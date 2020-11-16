using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Xsl;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Entity = CSharpFunctionalExtensions.Entity;

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
        public static Result<NuixConnection, IErrorBuilder> GetOrCreateNuixConnection(this StateMonad stateMonad)
        {
            var currentConnection = stateMonad.GetVariable<NuixConnection>(NuixVariableName);

            if (currentConnection.IsSuccess)
                return currentConnection.Value;

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
        public static Result<Unit, IErrorBuilder> CloseNuixConnection(this StateMonad stateMonad)
        {
            var currentConnection = stateMonad.GetVariable<NuixConnection>(NuixVariableName);

            if (currentConnection.IsFailure)
                return Unit.Default; //nothing to do

            try
            {
                currentConnection.Value.Dispose();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                return new ErrorBuilder(e, ErrorCode.ExternalProcessError);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return Unit.Default;
        }
    }

    /// <summary>
    /// An open connection to our general script running in Nuix
    /// </summary>
    public class NuixConnection : IDisposable
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
                var command = new ConnectorCommand
                {
                    Command = function.FunctionName
                };

                if (_evaluatedFunctions.Add(function.FunctionName))
                    command.FunctionDefinition = function.CompileFunctionText();

                var commandArguments = new Dictionary<string, object>();

                foreach (var argument in function.Arguments)
                {
                    if (parameters.TryGetValue(argument, out var value))
                    {
                        commandArguments.Add(argument.ParameterName, value);
                        //TODO special case for stream / entity stream
                    }
                }
                command.Arguments = commandArguments;

                // ReSharper disable once MethodHasAsyncOverload
                var commandJson = JsonConvert.SerializeObject(command, Formatting.None, EntityJsonConverter.Instance);

                await ExternalProcess.InputStream.WriteLineAsync(commandJson);

                var result = await GetOutputTyped<T>(logger, cancellationToken);

                return result;
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
                var output = await ExternalProcess.OutputStream.ReadLineAsync();

                if (output == null)
                    return new ErrorBuilder("Nuix process missing output", ErrorCode.ExternalProcessMissingOutput);

                //if (output.Value.source == StreamSource.Error)
                //    return new ErrorBuilder(output.Value.line, ErrorCode.ExternalProcessError);

                var jsonString = output.Value.line;


                ConnectorOutput connectorOutput;

                try
                {
                    connectorOutput= JsonConvert.DeserializeObject<ConnectorOutput>(jsonString);
                }
                catch (Exception e)
                {
                    return new ErrorBuilder(e, ErrorCode.CouldNotDeserialize);
                }

                if (connectorOutput.Error != null)
                    return new ErrorBuilder(connectorOutput.Error.Message, ErrorCode.ExternalProcessError);

                if(connectorOutput.Log != null)
                {
                    var severity = TryGetSeverity(connectorOutput.Log.Severity);

                    if (severity.IsFailure) return severity.ConvertFailure<T>();

                    switch (severity.Value)
                    {
                        case LogSeverity.Trace:
                            logger.LogTrace(connectorOutput.Log.Message);
                            break;
                        case LogSeverity.Information:
                            logger.LogInformation(connectorOutput.Log.Message);
                            break;
                        case LogSeverity.Warning:
                            logger.LogWarning(connectorOutput.Log.Message);
                            break;
                        case LogSeverity.Error:
                            logger.LogError(connectorOutput.Log.Message);
                            break;
                        case LogSeverity.Critical:
                            logger.LogCritical(connectorOutput.Log.Message);
                            break;
                        case LogSeverity.Debug:
                            logger.LogDebug(connectorOutput.Log.Message);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (connectorOutput.Result != null)
                {
                    if (Unit.Default is T u)
                        return u; //:)

                    if (connectorOutput.Result.Data is T t)
                        return t;

                    var convertedResult = Convert.ChangeType(connectorOutput.Result.Data, typeof(T));
                    if (convertedResult is T tConverted)
                        return tConverted;

                    return new ErrorBuilder($"Could not deserialize '{connectorOutput.Result.Data}' to {typeof(T).Name}", ErrorCode.CouldNotDeserialize);
                  
                }

            }

            return new ErrorBuilder("Process was cancelled", ErrorCode.ExternalProcessError);
        }

        private readonly HashSet<string> _evaluatedFunctions = new HashSet<string>();

        /// <inheritdoc />
        public void Dispose() => ExternalProcess.Dispose();

        private class ConnectorOutput
        {
            [JsonProperty("result")]
            public ConnectorOutputResult? Result { get; set; }

            [JsonProperty("log")]
            public ConnectorOutputLog? Log { get; set; }

            [JsonProperty("error")]
            public ConnectorOutputError? Error { get; set; }
        }

        private class ConnectorOutputLog
        {
            [JsonProperty("severity")]
            public string Severity { get; set; } = null!;

            [JsonProperty("message")]
            public string Message { get; set; } = null!;

            [JsonProperty("time")]
            public string Time { get; set; } = null!;

            [JsonProperty("stackTrace")]
            public string StackTrace { get; set; } = null!;

        }

        private static Result<LogSeverity, IErrorBuilder> TryGetSeverity(string s)
        {
            return (s.ToLowerInvariant()) switch
            {
                "trace" => LogSeverity.Trace,
                "info" => LogSeverity.Information,
                "warn" => LogSeverity.Warning,
                "error" => LogSeverity.Error,
                "fatal" => LogSeverity.Critical,
                "debug" => LogSeverity.Debug,
                _ => new ErrorBuilder($"Could not parse {s}", ErrorCode.CouldNotParse),
            };
        }

        private enum LogSeverity
        {
            [JsonProperty("trace")]
            Trace,
            [JsonProperty("info")]
            Information,
            [JsonProperty("warn")]
            Warning,
            [JsonProperty("error")]
            Error,
            [JsonProperty("fatal")]
            Critical,
            [JsonProperty("debug")]
            Debug
        }


        private class ConnectorOutputError
        {
            [JsonProperty("message")]
            public string Message { get; set; } = null!;
        }

        private class ConnectorOutputResult
        {
            [JsonProperty("data")]
            public object Data { get; set; } = null!;
        }


        private class ConnectorCommand
        {
            /// <summary>
            /// The command to send
            /// </summary>
            [JsonProperty("cmd")]
            public string Command { get; set; } = null!;

            [JsonProperty("def")]
            public string? FunctionDefinition { get; set; }

            [JsonProperty("args")]
            public Dictionary<string, object>? Arguments { get; set; }

            // ReSharper disable once StringLiteralTypo
            [JsonProperty("isstream")]
            public bool? IsStream { get; set; }
        }

        private class EntityJsonConverter : JsonConverter
        {
            private EntityJsonConverter() {}

            public static JsonConverter Instance { get; } = new EntityJsonConverter();

            /// <inheritdoc />
            public override void WriteJson(JsonWriter writer, object entityObject, JsonSerializer serializer)
            {
                if (!(entityObject is Reductech.EDR.Core.Entities.Entity entity))
                    return;

                var dictionary = new Dictionary<string, object?>();

                foreach (var (key, value) in entity)
                {
                    value.Value.Switch(
                        _ => { dictionary.Add(key, null); },
                        x => dictionary.Add(key, GetObject(x)),
                        x => dictionary.Add(key, GetList(x)));
                }

                serializer.Serialize(writer, dictionary);

                static List<object?> GetList(IEnumerable<EntitySingleValue> source)
                {
                    var r = source.Select(GetObject).ToList();
                    return r;
                }

                static object? GetObject(EntitySingleValue esv)
                {
                    object? o = null;

                    esv.Value.Switch(
                           a=> o= a,
                           a=> o= a,
                           a=> o= a,
                           a=> o= a,
                           a=> o= a,
                           a=> o= a
                           );

                    return o;
                }


            }

            /// <inheritdoc />
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public override bool CanConvert(Type objectType) => objectType == typeof(Entity);
        }
    }
}
