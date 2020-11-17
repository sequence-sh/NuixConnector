using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// Helps with connections to nuix.
    /// </summary>
    public static class NuixConnectionHelper
    {
        private const string NuixGeneralScriptName = "edr-nuix-connector.rb";

        private static readonly VariableName NuixVariableName = new VariableName("NuixConnection");

        /// <summary>
        /// Gets or creates a connection to nuix.
        /// </summary>
        public static Result<NuixConnection, IErrorBuilder> GetOrCreateNuixConnection(this StateMonad stateMonad, bool reopen)
        {
            var currentConnection = stateMonad.GetVariable<NuixConnection>(NuixVariableName);

            if (currentConnection.IsSuccess)
            {
                if (reopen)
                {
                    try
                    {
                        currentConnection.Value.ExternalProcess.WaitForExit(1000);
                        currentConnection.Value.Dispose(); //Get rid of this connection and open a new one
                    }
                    catch (InvalidOperationException) //Thrown if already disposed
                    {
                    }
                }
                    
                else
                    return currentConnection.Value;
            }

            var nuixSettingsResult = stateMonad.GetSettings<INuixSettings>();

            if (nuixSettingsResult.IsFailure) return nuixSettingsResult.ConvertFailure<NuixConnection>();

            var arguments = new List<string>();

            if (nuixSettingsResult.Value.UseDongle)
            {
                // ReSharper disable once StringLiteralTypo
                arguments.Add("-licencesourcetype");
                arguments.Add("dongle");
            }

            var scriptPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                NuixGeneralScriptName);

            arguments.Add(scriptPath);

            var r = stateMonad.ExternalProcessRunner.StartExternalProcess(nuixSettingsResult.Value.NuixExeConsolePath, arguments,
                Encoding.UTF8);

            if (r.IsFailure)
                return r.ConvertFailure<NuixConnection>();

            var connection = new NuixConnection(r.Value);

            var setResult = stateMonad.SetVariable(NuixVariableName, connection);

            if (setResult.IsFailure)
                return setResult.ConvertFailure<NuixConnection>()
                    .MapError(e=> ErrorBuilderList.Combine(e.GetAllErrors().Select(x=>new ErrorBuilder(x.Message, x.ErrorCode))));

            return connection;
        }
        /// <summary>
        /// Close the nuix connection if it is open.
        /// </summary>
        public static async Task<Result<Unit, IErrorBuilder>> CloseNuixConnectionAsync(this StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var currentConnection = stateMonad.GetVariable<NuixConnection>(NuixVariableName);

            if (currentConnection.IsFailure)
                return Unit.Default; //nothing to do

            try
            {
                await currentConnection.Value.SendDoneCommand(cancellationToken);

                currentConnection.Value.ExternalProcess.WaitForExit(1000);
                currentConnection.Value.Dispose();
            }
            catch (Exception e)
            {
                return new ErrorBuilder(e, ErrorCode.ExternalProcessError);
            }

            //TODO remove connection variable

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
        public NuixConnection(IExternalProcessReference externalProcess) => ExternalProcess = externalProcess;

        /// <summary>
        /// The nuix process.
        /// </summary>
        public IExternalProcessReference ExternalProcess { get; }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Sends the 'Done' command
        /// </summary>
        public async Task SendDoneCommand(CancellationToken cancellation)
        {
            var command = new ConnectionCommand()
            {
                Command = "done"
            };

            // ReSharper disable once MethodHasAsyncOverload
            var commandJson = JsonConvert.SerializeObject(command, Formatting.None, EntityJsonConverter.Instance, new StringEnumConverter());

            await ExternalProcess.InputChannel.WriteAsync(commandJson, cancellation);
        }

        /// <summary>
        /// Run a nuix function on this connection
        /// </summary>
        public async Task<Result<T, IErrorBuilder>> RunFunctionAsync<T>(
            ILogger logger,
            IRubyFunction<T> function,
            IReadOnlyDictionary<RubyFunctionParameter, object> parameters,
            CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                var command = new ConnectionCommand
                {
                    Command = function.FunctionName
                };

                if (_evaluatedFunctions.Add(function.FunctionName))
                    command.FunctionDefinition = function.CompileFunctionText();

                var commandArguments = new Dictionary<string, object>();

                EntityStream? entityStream = null;

                foreach (var argument in function.Arguments)
                {
                    if (parameters.TryGetValue(argument, out var value))
                    {
                        if (value is EntityStream sStream)
                        {
                            if(entityStream != null)
                                return new ErrorBuilder("Cannot have two entity stream parameters to a nuix function", ErrorCode.ExternalProcessError);

                            entityStream = sStream;
                            command.IsStream = true;
                        }
                        else
                            commandArguments.Add(argument.PropertyName, value);
                    }
                }

                command.Arguments = commandArguments;

                // ReSharper disable once MethodHasAsyncOverload
                var commandJson = JsonConvert.SerializeObject(command, Formatting.None, EntityJsonConverter.Instance, new StringEnumConverter());

                await ExternalProcess.InputChannel.WriteAsync(commandJson, cancellationToken);

                if (entityStream != null)
                {
                    var key = "--Stream--" + Guid.NewGuid();//random key as the stream opening / closing token
                    await ExternalProcess.InputChannel.WriteAsync(key, cancellationToken);
                    var entities = await entityStream.TryGetResultsAsync(cancellationToken);
                    if (entities.IsFailure)
                    {
                        await ExternalProcess.InputChannel.WriteAsync(key, cancellationToken);
                        return entities.ConvertFailure<T>().MapError(x=> new ErrorBuilder(x, ErrorCode.ExternalProcessError) as IErrorBuilder);
                    }

                    foreach (var entity in entities.Value)
                    {
                        var entityJson = JsonConvert.SerializeObject(entity, Formatting.None, EntityJsonConverter.Instance, new StringEnumConverter());

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


        private async Task<Result<T, IErrorBuilder>> GetOutputTyped<T>(ILogger logger, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string jsonString;
                StreamSource source;

                try
                {
                    (jsonString, source) = await ExternalProcess.OutputChannel.ReadAsync(cancellationToken);
                }
                catch (ChannelClosedException e)
                {
                    return new ErrorBuilder(e, ErrorCode.ExternalProcessError);
                }

                ConnectionOutput connectionOutput;

                try
                {
                    connectionOutput= JsonConvert.DeserializeObject<ConnectionOutput>(jsonString, EntityJsonConverter.Instance)!;
                }
                catch (Exception e)
                {
                    return new ErrorBuilder(e, ErrorCode.CouldNotDeserialize);
                }

                if (connectionOutput.Error != null)
                    return new ErrorBuilder(connectionOutput.Error.Message, ErrorCode.ExternalProcessError);

                if(connectionOutput.Log != null)
                {
                    if (source == StreamSource.Error)
                        return new ErrorBuilder(connectionOutput.Log.Message, ErrorCode.ExternalProcessError);

                    var severity = connectionOutput.Log.TryGetSeverity();

                    if (severity.IsFailure) return severity.ConvertFailure<T>();

                    switch (severity.Value)
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
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (connectionOutput.Result != null)
                {
                    if (Unit.Default is T u)
                        return u; //:)

                    if (connectionOutput.Result.Data is T t)
                        return t;

                    var convertedResult = Convert.ChangeType(connectionOutput.Result.Data, typeof(T));
                    if (convertedResult is T tConverted)
                        return tConverted;

                    return new ErrorBuilder($"Could not deserialize '{connectionOutput.Result.Data}' to {typeof(T).Name}", ErrorCode.CouldNotDeserialize);

                }

            }

            return new ErrorBuilder("Process was cancelled", ErrorCode.ExternalProcessError);
        }

        private readonly HashSet<string> _evaluatedFunctions = new HashSet<string>();

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                ExternalProcess.Dispose();
            }
            catch (InvalidOperationException) //Thrown if the process was already disposed
            {
            }
        }
    }
}
