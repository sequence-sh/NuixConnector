using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Sdk;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

internal class ExternalProcessMock : IExternalProcessRunner
{
    public string ProcessPath { get; set; } = "TestPath";

    public List<string> ProcessArgs { get; set; } = new()
    {
        "-licencesourcetype", "dongle", NuixConnectionHelper.NuixGeneralScriptName
    };

    public Encoding ProcessEncoding { get; set; } = Encoding.UTF8;

    public bool ValidateArguments { get; set; } = true;

    public ExternalProcessMock(
        int expectedTimesStarted,
        params ExternalProcessAction[] externalProcessActions)
    {
        ExpectedTimesStarted   = expectedTimesStarted;
        ExternalProcessActions = externalProcessActions;
    }

    public int ExpectedTimesStarted { get; }
    public ExternalProcessAction[] ExternalProcessActions { get; }

    public int TimesStarted { get; private set; }

    /// <inheritdoc />
    public async Task<Result<Unit, IErrorBuilder>> RunExternalProcess(
        string processPath,
        IErrorHandler errorHandler,
        IEnumerable<string> arguments,
        IReadOnlyDictionary<string, string> environmentVariables,
        Encoding encoding,
        IStateMonad stateMonad,
        IStep? callingStep,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        throw new XunitException("nuix processes should not RunExternalProcess");
    }

    /// <inheritdoc />
    public Result<IExternalProcessReference, IErrorBuilder> StartExternalProcess(
        string processPath,
        IEnumerable<string> arguments,
        IReadOnlyDictionary<string, string> environmentVariables,
        Encoding encoding,
        IStateMonad stateMonad,
        IStep? callingStep)
    {
        var args = arguments.ToList();

        TimesStarted++;

        if (TimesStarted > ExpectedTimesStarted)
            throw new XunitException(
                $"Should only start external process {ExpectedTimesStarted} times"
            );

        if (ValidateArguments)
        {
            processPath.Should().Be(ProcessPath);
            encoding.Should().Be(ProcessEncoding);
            args[0].Should().Be(ProcessArgs[0]);
            args[1].Should().Be(ProcessArgs[1]);
            args[2].Should().EndWith(ProcessArgs[2]);
        }
        else
        {
            if (!processPath.Equals(ProcessPath))
                return new ErrorBuilder(
                    ErrorCode.ExternalProcessError,
                    $"Could not start '{processPath}'"
                );
        }

        return new ProcessReferenceMock(ExternalProcessActions);
    }

    internal class ProcessReferenceMock : IExternalProcessReference
    {
        public ProcessReferenceMock(params ExternalProcessAction[] externalProcessActions)
        {
            RemainingExternalProcessActions =
                new Stack<ExternalProcessAction>(externalProcessActions.Reverse());

            var iChannel = Channel.CreateUnbounded<string>();
            var oChannel = Channel.CreateUnbounded<(string line, StreamSource source)>();

            InputChannel             = iChannel.Writer;
            OutputChannel            = oChannel.Reader;
            _cancellationTokenSource = new CancellationTokenSource();

            //This method will run in another thread.
            _ = ReadInput(
                iChannel.Reader,
                oChannel.Writer,
                RemainingExternalProcessActions,
                _cancellationTokenSource.Token
            );
        }

        private static async Task ReadInput(
            ChannelReader<string> inputChannel,
            ChannelWriter<(string line, StreamSource source)> output,
            Stack<ExternalProcessAction> externalProcessActions,
            CancellationToken cancellationToken)
        {
            var     isStream       = false;
            string? streamEndToken = null;
            var     entityStream   = new List<string>();

            await foreach (var inputJson in inputChannel.ReadAllAsync(cancellationToken))
            {
                try
                {
                    if (isStream)
                    {
                        if (streamEndToken is null)
                        {
                            streamEndToken = inputJson;
                        }
                        else if (streamEndToken.Equals(inputJson))
                        {
                            isStream       = false;
                            streamEndToken = null;

                            var data = new ConnectionOutput
                            {
                                Result = new ConnectionOutputResult
                                {
                                    Data = $"[{string.Join(',', entityStream)}]"
                                }
                            };

                            var json = JsonSerializer.Serialize(data);
                            await output.WriteAsync((json, StreamSource.Output), cancellationToken);
                        }
                        else
                        {
                            entityStream.Add(inputJson);
                        }

                        continue;
                    }

                    if (!externalProcessActions.TryPop(out var expectedAction))
                        throw new XunitException($"Unexpected: '{inputJson}'");

                    var commandResult = DeserializeConnectionCommand(inputJson);
                    commandResult.ShouldBeSuccessful();

                    commandResult.Value.Should()
                        .BeEquivalentTo(
                            expectedAction.Command,
                            option =>
                                option.Excluding(su => su.FunctionDefinition)
                        );

                    if (commandResult.Value.IsStream != null && commandResult.Value.IsStream.Value)
                    {
                        isStream = true;
                    }

                    if (expectedAction.WriteToStdOut != null)
                        foreach (var msg in expectedAction.WriteToStdOut)
                            await output.WriteAsync((msg, StreamSource.Output), cancellationToken);

                    if (expectedAction.WriteToStdErr != null)
                        foreach (var msg in expectedAction.WriteToStdErr)
                            await output.WriteAsync((msg, StreamSource.Error), cancellationToken);

                    foreach (var connectionOutput in expectedAction.DesiredOutput)
                    {
                        if (isStream && !(connectionOutput.Result is null))
                            throw new XunitException(
                                "Stream functions cannot have 'Result' set in ConnectionOutput"
                            );

                        var json = JsonSerializer.Serialize(
                            connectionOutput,
                            JsonConverters.Options
                        );

                        await output.WriteAsync((json, StreamSource.Output), cancellationToken);
                    }

                    if (commandResult.Value.Command.Equals("done"))
                        output.Complete();
                }
                catch (Exception e)
                {
                    var exception = e;

                    while (exception.InnerException != null)
                        exception = exception.InnerException;

                    var error = new ConnectionOutput
                    {
                        Error = new ConnectionOutputError { Message = exception.Message }
                    };

                    var errorJson = JsonSerializer.Serialize(error);

                    await output.WriteAsync((errorJson, StreamSource.Error), cancellationToken);
                }
            }
        }

        private readonly CancellationTokenSource _cancellationTokenSource;

        private Stack<ExternalProcessAction> RemainingExternalProcessActions { get; }

        internal bool IsDisposed { get; private set; }

        /// <inheritdoc />
        public void Dispose()
        {
            if (IsDisposed)
                throw new InvalidOperationException("Already disposed.");

            IsDisposed = true;
            _cancellationTokenSource.Cancel();
        }

        /// <inheritdoc />
        public void WaitForExit(int? milliseconds)
        {
            Thread.Sleep(new Random().Next(100));
        }

        /// <inheritdoc />
        public ChannelReader<(string line, StreamSource source)> OutputChannel { get; }

        /// <inheritdoc />
        public ChannelWriter<string> InputChannel { get; }
    }

    /// <summary>
    /// Deserialize a json string into a ConnectionCommand
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static Result<ConnectionCommand, IErrorBuilder> DeserializeConnectionCommand(string json)
    {
        try
        {
            var command1 =
                JsonSerializer.Deserialize<ConnectionCommand>(json, JsonConverters.Options)!;

            if (command1.Arguments == null)
                return command1;

            var newArguments = new Dictionary<string, object>();

            foreach (var (key, value) in command1.Arguments)
            {
                object newValue;

                if (value is JsonElement jElement)
                {
                    newValue = JsonConverters.ConvertToObject(jElement);
                }
                else
                    newValue = value;

                newArguments.Add(key, newValue);
            }

            var command2 = new ConnectionCommand
            {
                Arguments          = newArguments,
                Command            = command1.Command,
                FunctionDefinition = command1.FunctionDefinition,
                IsStream           = command1.IsStream
            };

            return command2;
        }
        catch (JsonException e)
        {
            return new ErrorBuilder(e, ErrorCode.ExternalProcessError);
        }
    }
}

}
