using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reductech.EDR.Connectors.Nuix.Errors;
using Reductech.EDR.Connectors.Nuix.Logging;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// An open connection to our general script running in Nuix
/// </summary>
public sealed class NuixConnection : IDisposable, IStateDisposable
{
    /// <summary>
    /// Create a new NuixConnection
    /// </summary>
    public NuixConnection(
        IExternalProcessReference externalProcess,
        NuixConnectionSettings nuixConnectionSettings)
    {
        ExternalProcess    = externalProcess;
        ConnectionSettings = nuixConnectionSettings;
    }

    /// <summary>
    /// The nuix process.
    /// </summary>
    public IExternalProcessReference ExternalProcess { get; }

    /// <summary>
    /// The Nuix Connection Settings
    /// </summary>
    public NuixConnectionSettings ConnectionSettings { get; }

    /// <summary>
    /// Returns true if the underlying connection has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    private Maybe<string> CurrentCasePath { get; set; } = Maybe<string>.None;

    private readonly SemaphoreSlim _semaphore = new(1);

    /// <summary>
    /// Sends the 'Done' command
    /// </summary>
    public async Task SendDoneCommand(
        IStateMonad state,
        IStep? step,
        CancellationToken cancellation)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(NuixConnection));

        var command = new ConnectionCommand() { Command = "done" };

        // ReSharper disable once MethodHasAsyncOverload
        var commandJson = JsonConvert.SerializeObject(command, Formatting.None, JsonConverters.All);

        await ExternalProcess.InputChannel.WriteAsync(commandJson, cancellation);

        // Log any remaining messages
        await GetOutputTyped<string>(state, step, cancellation);
    }

    /// <summary>
    /// Run a nuix function on this connection
    /// </summary>
    public async Task<Result<T, IErrorBuilder>> RunFunctionAsync<T>(
        IStateMonad stateMonad,
        IStep? step,
        RubyFunction<T> function,
        IReadOnlyDictionary<RubyFunctionParameter, object> parameters,
        CasePathParameter casePathParameter,
        CancellationToken cancellationToken)
    {
        string? casePath;

        switch (casePathParameter)
        {
            case CasePathParameter.IgnoresOpenCase:
                casePath = null;
                break;

            case CasePathParameter.ChangesOpenCase { NewCaseParameter: { HasNoValue: true } }:
            {
                CurrentCasePath = Maybe<string>.None; //This will be the case path for the next step
                casePath        = null;
                break;
            }
            case CasePathParameter.ChangesOpenCase
                { NewCaseParameter: { HasValue: true } } opensCase:
            {
                if (parameters.TryGetValue(opensCase.NewCaseParameter.Value, out var cp))
                {
                    CurrentCasePath = cp.ToString()!; //This will be the case path for the next step
                    casePath        = null;
                }
                else
                    return new ErrorBuilder(
                        ErrorCode.MissingParameter,
                        opensCase.NewCaseParameter.Value.PropertyName
                    );

                break;
            }
            case CasePathParameter.UsesCase usesCase:
            {
                if (parameters.TryGetValue(usesCase.CaseParameter, out var cp))
                {
                    casePath        = cp.ToString()!;
                    CurrentCasePath = casePath;
                }
                else if (CurrentCasePath.HasValue)
                    casePath = CurrentCasePath.Value;
                else
                    return new ErrorBuilder(ErrorCode_Nuix.NoCaseOpen);

                break;
            }
            default: throw new ArgumentOutOfRangeException(nameof(casePathParameter));
        }

        if (IsDisposed)
            throw new ObjectDisposedException(nameof(NuixConnection));

        await _semaphore.WaitAsync(cancellationToken);

        if (function.RequiredHelpers is { Count: > 0 })
        {
            foreach (var helper in function.RequiredHelpers)
            {
                if (_evaluatedFunctions.Add(helper.FunctionName))
                {
                    var helperCommand = new ConnectionCommand
                    {
                        Command            = helper.FunctionName,
                        FunctionDefinition = helper.FunctionText,
                        IsHelper           = true
                    };

                    // ReSharper disable once MethodHasAsyncOverload
                    var helperCommandJson = JsonConvert.SerializeObject(
                        helperCommand,
                        Formatting.None,
                        JsonConverters.All
                    );

                    await ExternalProcess.InputChannel.WriteAsync(
                        helperCommandJson,
                        cancellationToken
                    );

                    var helperResult = await GetOutputTyped<T>(stateMonad, step, cancellationToken);

                    if (helperResult.IsFailure)
                        return helperResult.ConvertFailure<T>();
                }
                else
                {
                    LogSituationNuix.HelperExists.Log(stateMonad, step, helper.FunctionName);
                }
            }
        }

        try
        {
            var command = new ConnectionCommand
            {
                Command = function.FunctionName, CasePath = casePath
            };

            if (_evaluatedFunctions.Add(function.FunctionName))
                command.FunctionDefinition = function.CompileFunctionText();

            var commandArguments = new Dictionary<string, object>();

            Array<Entity>? entityStream = null;

            foreach (var argument in function.Arguments)
            {
                if (parameters.TryGetValue(argument, out var value))
                {
                    if (value is Array<Entity> sStream)
                    {
                        if (entityStream != null)
                            return new ErrorBuilder(ErrorCode_Nuix.TooManyEntityStreams);

                        entityStream     = sStream;
                        command.IsStream = true;
                    }
                    else
                        commandArguments.Add(argument.PropertyName, value);
                }
            }

            command.Arguments = commandArguments;

            // ReSharper disable once MethodHasAsyncOverload
            var commandJson = JsonConvert.SerializeObject(
                command,
                Formatting.None,
                JsonConverters.All
            );

            await ExternalProcess.InputChannel.WriteAsync(commandJson, cancellationToken);

            if (entityStream != null)
            {
                var key = "--Stream--"
                        + Guid.NewGuid(); //random key as the stream opening / closing token

                await ExternalProcess.InputChannel.WriteAsync(key, cancellationToken);
                var entities = await entityStream.GetElementsAsync(cancellationToken);

                if (entities.IsFailure)
                {
                    await ExternalProcess.InputChannel.WriteAsync(key, cancellationToken);

                    return entities.ConvertFailure<T>()
                        .MapError(
                            x => new ErrorBuilder(
                                ErrorCode.ExternalProcessError,
                                x.AsString
                            ) as IErrorBuilder
                        );
                }

                foreach (var entity in entities.Value)
                {
                    var entityJson = JsonConvert.SerializeObject(
                        entity,
                        Formatting.None,
                        JsonConverters.All
                    );

                    await ExternalProcess.InputChannel.WriteAsync(entityJson, cancellationToken);
                }

                await ExternalProcess.InputChannel.WriteAsync(key, cancellationToken);
            }

            var result = await GetOutputTyped<T>(stateMonad, step, cancellationToken);

            return result;
        }
        catch (Exception e)
        {
            return new ErrorBuilder(e, ErrorCode.ExternalProcessError);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<Result<T, IErrorBuilder>> GetOutputTyped<T>(
        IStateMonad stateMonad,
        IStep? callingStep,
        CancellationToken cancellationToken,
        bool returnOnLog = false)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            string       jsonString;
            StreamSource source;

            try
            {
                (jsonString, source) =
                    await ExternalProcess.OutputChannel.ReadAsync(cancellationToken);
            }
            catch (ChannelClosedException e)
            {
                return new ErrorBuilder(
                    ErrorCode.ExternalProcessError,
                    $"{e.Message} - there may have been an authentication error."
                );
            }

            ConnectionOutput connectionOutput;

            try
            {
                connectionOutput = JsonConvert.DeserializeObject<ConnectionOutput>(
                    jsonString,
                    JsonConverters.All
                )!;
            }
            catch (Exception)
            {
                var warningMatch = ConnectionSettings.JavaWarningRegex.Match(jsonString);
                var errorMatch   = ConnectionSettings.JavaErrorRegex.Match(jsonString);

                if (warningMatch.Success)
                {
                    connectionOutput = new ConnectionOutput
                    {
                        Log = new ConnectionOutputLog
                        {
                            Message = warningMatch.Groups["text"].Value, Severity = "warn"
                        }
                    };

                    source = StreamSource.Output; //Filthy hack
                }
                else if (errorMatch.Success)
                {
                    connectionOutput = new ConnectionOutput
                    {
                        Log = new ConnectionOutputLog
                        {
                            Message = errorMatch.Groups["text"].Value, Severity = "warn"
                        }
                    };

                    source = StreamSource.Output; //Filthy hack
                }
                else
                {
                    return new ErrorBuilder(
                        ErrorCode.CouldNotParse,
                        jsonString,
                        nameof(ConnectionOutput)
                    );
                }
            }

            var valid = connectionOutput.Validate();

            if (valid.IsFailure)
                return valid.ConvertFailure<T>();

            if (connectionOutput.Error != null)
                return new ErrorBuilder(
                    ErrorCode.ExternalProcessError,
                    connectionOutput.Error.Message
                );

            if (connectionOutput.Log != null)
            {
                if (source == StreamSource.Error)
                    return new ErrorBuilder(
                        ErrorCode.ExternalProcessError,
                        connectionOutput.Log.Message
                    );

                var severity = connectionOutput.Log.TryGetSeverity();

                if (severity.IsFailure)
                    return severity.ConvertFailure<T>();

                var logLevel = severity.Value.ToLogLevel();

                stateMonad.Log(logLevel, connectionOutput.Log.Message, callingStep);

                if (returnOnLog)
                    return new Result<T, IErrorBuilder>();

                continue;
            }

            if (connectionOutput.Result != null)
            {
                if (Unit.Default is T u)
                    return u; //:)

                if (connectionOutput.Result.Data is T t)
                    return t;

                if (typeof(T) == typeof(StringStream))
                {
                    var ss = new StringStream(connectionOutput.Result.Data?.ToString()!);

                    if (ss is T typedStringStream) //Should always be true
                        return typedStringStream;
                }

                if (typeof(T) == typeof(Entity) && connectionOutput.Result.Data is JObject jo)
                {
                    if (Entity.Create(jo) is T entity)
                        return entity;
                }
                else
                {
                    var convertedResult = Convert.ChangeType(
                        connectionOutput.Result.Data,
                        typeof(T)
                    );

                    if (convertedResult is T tConverted)
                        return tConverted;
                }

                return new ErrorBuilder(
                    ErrorCode.CouldNotParse,
                    connectionOutput.Result.Data,
                    typeof(T).Name
                );
            }
        }

        return new ErrorBuilder(ErrorCode.ExternalProcessError, "Process was cancelled");
    }

    private readonly HashSet<string> _evaluatedFunctions = new();

    /// <inheritdoc />
    public void Dispose()
    {
        if (!IsDisposed)
        {
            ExternalProcess.Dispose();
            IsDisposed = true;
        }
    }

    /// <inheritdoc />
    public async Task DisposeAsync(IStateMonad state)
    {
        if (!IsDisposed)
        {
            await SendDoneCommand(state, null, CancellationToken.None);
            ExternalProcess.Dispose();
            IsDisposed = true;
        }
    }
}

}
