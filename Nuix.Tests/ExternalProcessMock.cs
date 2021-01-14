using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit.Sdk;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

internal class ExternalProcessMock : IExternalProcessRunner
{
    public string ProcessPath { get; set; } = "TestPath";

    public List<string> ProcessArgs { get; set; } = new List<string>
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

    public int TimesStarted { get; private set; } = 0;

    /// <inheritdoc />
    public async Task<Result<Unit, IErrorBuilder>> RunExternalProcess(
        string processPath,
        ILogger logger,
        IErrorHandler errorHandler,
        IEnumerable<string> arguments,
        Encoding encoding,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        throw new XunitException("nuix processes should not RunExternalProcess");
    }

    /// <inheritdoc />
    public Result<IExternalProcessReference, IErrorBuilder> StartExternalProcess(
        string processPath,
        IEnumerable<string> arguments,
        Encoding encoding)
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

                            var data = new ConnectionOutput()
                            {
                                Result = new ConnectionOutputResult()
                                {
                                    Data = $"[{string.Join(',', entityStream)}]"
                                }
                            };

                            var json = JsonConvert.SerializeObject(data);
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

                    var commandResult = JsonConverters.DeserializeConnectionCommand(inputJson);
                    commandResult.ShouldBeSuccessful(x => x.AsString);

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

                        var json = JsonConvert.SerializeObject(connectionOutput);
                        await output.WriteAsync((json, StreamSource.Output), cancellationToken);
                    }
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

                    var errorJson = JsonConvert.SerializeObject(error);

                    await output.WriteAsync((errorJson, StreamSource.Error), cancellationToken);
                }
            }
        }

        private readonly CancellationTokenSource _cancellationTokenSource;

        private Stack<ExternalProcessAction> RemainingExternalProcessActions { get; }

        internal bool IsDisposed { get; private set; } = false;

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
}

}
