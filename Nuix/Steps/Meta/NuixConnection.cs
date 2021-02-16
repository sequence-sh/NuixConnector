using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Reductech.EDR.Connectors.Nuix.Errors;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// Helps with connections to nuix.
/// </summary>
public static class NuixConnectionHelper
{
    internal const string NuixGeneralScriptName = "nuixconnectorscript.rb";

    internal static readonly VariableName NuixVariableName =
        new("ReductechNuixConnection");

    /// <summary>
    /// Gets or creates a connection to nuix.
    /// </summary>
    public static Result<NuixConnection, IErrorBuilder> GetOrCreateNuixConnection(
        this IStateMonad stateMonad,
        bool reopen)
    {
        var currentConnection = stateMonad.GetVariable<NuixConnection>(NuixVariableName);

        if (currentConnection.IsSuccess)
        {
            // TODO: What happens if the connection is closed/disposed?
            if (reopen)
            {
                if (currentConnection.Value.IsDisposed)
                {
                    stateMonad.Logger.LogDebug("Connection already disposed.");
                }
                else
                {
                    currentConnection.Value.ExternalProcess.WaitForExit(1000);

                    currentConnection.Value
                        .Dispose(); //Get rid of this connection and open a new one
                }
            }

            else
                return currentConnection.Value;
        }

        var settings = stateMonad.Settings;

        var consoleArguments = TryGetConsoleArguments(settings);

        if (consoleArguments.IsFailure)
            return consoleArguments.ConvertFailure<NuixConnection>();

        // TODO: Make this configurable
        var scriptPath = Path.Combine(AppContext.BaseDirectory, NuixGeneralScriptName);

        if (!stateMonad.ExternalContext.FileSystemHelper.File.Exists(scriptPath))
            return ErrorCode.ExternalProcessNotFound.ToErrorBuilder(scriptPath);

        consoleArguments.Value.arguments.Add(scriptPath);

        var environmentVariables = TryGetEnvironmentVariables(settings);

        if (environmentVariables.IsFailure)
            return environmentVariables.ConvertFailure<NuixConnection>();

        var r = stateMonad.ExternalContext.ExternalProcessRunner.StartExternalProcess(
            consoleArguments.Value.consolePath,
            consoleArguments.Value.arguments,
            environmentVariables.Value,
            Encoding.UTF8,
            stateMonad.Logger
        );

        if (r.IsFailure)
            return r.ConvertFailure<NuixConnection>();

        var connection = new NuixConnection(r.Value);

        var setResult = stateMonad.SetVariable(NuixVariableName, connection);

        if (setResult.IsFailure)
            return setResult.ConvertFailure<NuixConnection>()
                .MapError(x => x.ToErrorBuilder);

        return connection;
    }

    private static Result<IReadOnlyDictionary<string, string>, IErrorBuilder>
        TryGetEnvironmentVariables(SCLSettings sclSettings)
    {
        var ev =
            sclSettings.Entity.TryGetValue(
                new EntityPropertyKey(
                    new[]
                    {
                        SCLSettings.ConnectorsKey, NuixSettings.NuixSettingsKey,
                        NuixSettings.EnvironmentVariablesKey
                    }
                )
            );

        if (ev.HasNoValue)
            return new Dictionary<string, string>();

        if (!ev.Value.TryPickT7(out var entity, out _))
            return ErrorCode.MissingStepSettingsValue.ToErrorBuilder(
                NuixSettings.NuixSettingsKey,
                NuixSettings.EnvironmentVariablesKey
            );

        var dict = new Dictionary<string, string>();

        foreach (var ep in entity)
            dict.Add(ep.Name, ep.BestValue.GetString());

        var username = sclSettings.Entity.TryGetNestedString(
            SCLSettings.ConnectorsKey,
            NuixSettings.NuixSettingsKey,
            NuixSettings.NuixUsernameKey
        );

        if (username.HasValue)
            dict.Add(NuixSettings.NuixUsernameKey, username.Value);

        var password = sclSettings.Entity.TryGetNestedString(
            SCLSettings.ConnectorsKey,
            NuixSettings.NuixSettingsKey,
            NuixSettings.NuixPasswordKey
        );

        if (password.HasValue)
            dict.Add(NuixSettings.NuixPasswordKey, password.Value);

        return dict;
    }

    /// <summary>
    /// Get the arguments that will be sent to the nuix console
    /// </summary>
    public static Result<(string consolePath, List<string> arguments), IErrorBuilder>
        TryGetConsoleArguments(SCLSettings sclSettings)
    {
        var pathResult = sclSettings.Entity.TryGetNestedString(
            SCLSettings.ConnectorsKey,
            NuixSettings.NuixSettingsKey,
            NuixSettings.ConsolePathKey
        );

        if (pathResult.HasNoValue)
            return ErrorCode.MissingStepSettingsValue.ToErrorBuilder(
                NuixSettings.NuixSettingsKey,
                NuixSettings.ConsolePathKey
            );

        var argumentsList = new List<string>();

        void MaybeAddBoolValue(string key)
        {
            var b = sclSettings.Entity.TryGetNestedBool(
                SCLSettings.ConnectorsKey,
                NuixSettings.NuixSettingsKey,
                key
            );

            if (b)
                argumentsList.Add("-" + key);
        }

        void MaybeAddStringKey(string key)
        {
            var s = sclSettings.Entity.TryGetNestedString(
                SCLSettings.ConnectorsKey,
                NuixSettings.NuixSettingsKey,
                key
            );

            if (s.HasValue)
            {
                argumentsList.Add("-" + key);
                argumentsList.Add(s.Value);
            }
        }

        var extraConsoleArguments =
            sclSettings.Entity.TryGetNestedList(
                SCLSettings.ConnectorsKey,
                NuixSettings.NuixSettingsKey,
                NuixSettings.ConsoleArgumentsKey
            );

        if (extraConsoleArguments.HasValue)
            argumentsList.AddRange(extraConsoleArguments.Value);

        MaybeAddBoolValue(NuixSettings.SignoutKey);
        MaybeAddBoolValue(NuixSettings.ReleaseKey);
        MaybeAddStringKey(NuixSettings.LicenceSourceTypeKey);
        MaybeAddStringKey(NuixSettings.LicenceSourceLocationKey);
        MaybeAddStringKey(NuixSettings.LicenceTypeKey);
        MaybeAddStringKey(NuixSettings.LicenceWorkersKey);

        var postConsoleArguments =
            sclSettings.Entity.TryGetNestedList(
                SCLSettings.ConnectorsKey,
                NuixSettings.NuixSettingsKey,
                NuixSettings.ConsoleArgumentsPostKey
            );

        if (postConsoleArguments.HasValue)
            argumentsList.AddRange(postConsoleArguments.Value);

        return (pathResult.Value, argumentsList);
    }

    /// <summary>
    /// Close the nuix connection if it is open.
    /// </summary>
    public static async Task<Result<Unit, IErrorBuilder>> CloseNuixConnectionAsync(
        this IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var currentConnection = stateMonad.GetVariable<NuixConnection>(NuixVariableName);

        if (currentConnection.IsFailure)
            return Unit.Default; //nothing to do

        try
        {
            await currentConnection.Value.SendDoneCommand(stateMonad, cancellationToken);

            currentConnection.Value.ExternalProcess.WaitForExit(1000);
            currentConnection.Value.Dispose();
        }
        catch (Exception e)
        {
            return new ErrorBuilder(e, ErrorCode.ExternalProcessError);
        }

        stateMonad.RemoveVariable(NuixVariableName, false);

        return Unit.Default;
    }
}

/// <summary>
/// An open connection to our general script running in Nuix
/// </summary>
public sealed class NuixConnection : IDisposable
{
    /// <summary>
    /// Create a new NuixConnection
    /// </summary>
    public NuixConnection(IExternalProcessReference externalProcess) =>
        ExternalProcess = externalProcess;

    /// <summary>
    /// The nuix process.
    /// </summary>
    public IExternalProcessReference ExternalProcess { get; }

    /// <summary>
    /// Returns true if the underlying connection has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    private Maybe<string> CurrentCasePath { get; set; } = Maybe<string>.None;

    private readonly SemaphoreSlim _semaphore = new(1);

    /// <summary>
    /// Sends the 'Done' command
    /// </summary>
    public async Task SendDoneCommand(IStateMonad state, CancellationToken cancellation)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(NuixConnection));

        var command = new ConnectionCommand() { Command = "done" };

        // ReSharper disable once MethodHasAsyncOverload
        var commandJson = JsonConvert.SerializeObject(command, Formatting.None, JsonConverters.All);

        await ExternalProcess.InputChannel.WriteAsync(commandJson, cancellation);

        // Log the ack
        await GetOutputTyped<Unit>(state.Logger, cancellation, true);
    }

    /// <summary>
    /// Run a nuix function on this connection
    /// </summary>
    public async Task<Result<T, IErrorBuilder>> RunFunctionAsync<T>(
        ILogger logger,
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

            case CasePathParameter.ChangesOpenCase opensCase
                when opensCase.NewCaseParameter.HasNoValue:
            {
                CurrentCasePath = Maybe<string>.None; //This will be the case path for the next step
                casePath        = null;
                break;
            }
            case CasePathParameter.ChangesOpenCase opensCase
                when opensCase.NewCaseParameter.HasValue:
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

            var result = await GetOutputTyped<T>(logger, cancellationToken);

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

    private static readonly Regex JavaWarningRegex = new(
        @"\(eval\):9: warning:(?<text>.+)",
        RegexOptions.Compiled
    );

    private static readonly Regex JavaErrorRegex = new(@"ERROR\s*(?<text>.+)");

    private async Task<Result<T, IErrorBuilder>> GetOutputTyped<T>(
        ILogger logger,
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
                connectionOutput =
                    JsonConvert.DeserializeObject<ConnectionOutput>(
                        jsonString,
                        JsonConverters.All
                    )!;
            }
            catch (Exception)
            {
                var warningMatch = JavaWarningRegex.Match(jsonString);
                var errorMatch   = JavaErrorRegex.Match(jsonString);

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

                switch (severity.Value) //TODO replace with enumerated log messages
                {
                    case LogSeverity.Trace:
                        logger.LogTrace(connectionOutput.Log.Message);
                        break;
                    case LogSeverity.Information:
                        logger.LogInformation(connectionOutput.Log.Message);
                        break;
                    case LogSeverity.Warning:
                        logger.LogWarning(connectionOutput.Log.Message);
                        break;
                    case LogSeverity.Error:
                        logger.LogError(connectionOutput.Log.Message);
                        break;
                    case LogSeverity.Critical:
                        logger.LogCritical(connectionOutput.Log.Message);
                        break;
                    case LogSeverity.Debug:
                        logger.LogDebug(connectionOutput.Log.Message);
                        break;
                    default: throw new InvalidEnumArgumentException();
                }

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

                var convertedResult = Convert.ChangeType(connectionOutput.Result.Data, typeof(T));

                if (convertedResult is T tConverted)
                    return tConverted;

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
}

}
