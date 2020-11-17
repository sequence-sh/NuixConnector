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
        public ExternalProcessMock(int expectedTimesStarted, params ExternalProcessAction[] externalProcessActions)
        {
            ExpectedTimesStarted = expectedTimesStarted;
            ExternalProcessActions = externalProcessActions;
        }

        public int ExpectedTimesStarted { get; }
        public ExternalProcessAction[] ExternalProcessActions { get; }

        public int TimesStarted { get; private set; } = 0;

        /// <inheritdoc />
        public async Task<Result<Unit, IErrorBuilder>> RunExternalProcess(string processPath, ILogger logger, IErrorHandler errorHandler, IEnumerable<string> arguments,
            Encoding encoding, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            throw new XunitException("nuix processes should not RunExternalProcess");
        }

        /// <inheritdoc />
        public Result<IExternalProcessReference, IErrorBuilder> StartExternalProcess(string processPath, IEnumerable<string> arguments, Encoding encoding)
        {
            var args = arguments.ToList();

            TimesStarted++;
            if(TimesStarted > ExpectedTimesStarted)
                throw new XunitException($"Should only start external process {ExpectedTimesStarted} times");

            processPath.Should().Be("TestPath");

            encoding.Should().Be(Encoding.UTF8);

            args[0].Should().Be("-licencesourcetype");
            args[1].Should().Be("dongle");
            args[2].Should().EndWith("edr-nuix-connector.rb");

            return new ProcessReferenceMock(ExternalProcessActions);
        }

        private class ProcessReferenceMock : IExternalProcessReference
        {

            public ProcessReferenceMock(params ExternalProcessAction[] externalProcessActions)
            {
                RemainingExternalProcessActions = new Stack<ExternalProcessAction>(externalProcessActions.Reverse());
                var iChannel = Channel.CreateUnbounded<string>();
                var oChannel = Channel.CreateUnbounded<(string line, StreamSource source)>();

                InputChannel = iChannel.Writer;
                OutputChannel = oChannel.Reader;
                _cancellationTokenSource = new CancellationTokenSource();

                //This method will run in another thread.
                _ = ReadInput(iChannel.Reader, oChannel.Writer, RemainingExternalProcessActions, _cancellationTokenSource.Token);

            }

            private static async Task ReadInput(ChannelReader<string> commandChannel,
                ChannelWriter<(string line, StreamSource source)> output,
                Stack<ExternalProcessAction> externalProcessActions,
                CancellationToken cancellationToken)
            {
                await foreach (var commandJson in commandChannel.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        if (!externalProcessActions.TryPop(out var expectedAction))
                            throw new XunitException($"Unexpected: '{commandJson}'");

                        var commandResult = EntityJsonConverter.DeserializeConnectionCommand(commandJson);
                        commandResult.ShouldBeSuccessful(x=>x.AsString);

                        commandResult.Value.Should().BeEquivalentTo(expectedAction.Command,
                            option=>
                                option.Excluding(su => su.FunctionDefinition)
                        );


                        foreach (var connectionOutput in expectedAction.DesiredOutput)
                        {
                            var json = JsonConvert.SerializeObject(connectionOutput);

                            await output.WriteAsync((json, StreamSource.Output), cancellationToken);
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception e)
                    {
                        var exception = e;
                        while (exception.InnerException != null) exception = exception.InnerException;

                        var error = new ConnectionOutput{Error = new ConnectionOutputError{Message = exception.Message}};
                        var errorJson = JsonConvert.SerializeObject(error);


                        await output.WriteAsync((errorJson, StreamSource.Error), cancellationToken);
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                }
            }

            private readonly CancellationTokenSource _cancellationTokenSource;

            private Stack<ExternalProcessAction>  RemainingExternalProcessActions { get; }
            /// <inheritdoc />
            public void Dispose()
            {
                _cancellationTokenSource.Cancel();
            }

            /// <inheritdoc />
            public void WaitForExit(int? milliseconds)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public ChannelReader<(string line, StreamSource source)> OutputChannel { get; }

            /// <inheritdoc />
            public ChannelWriter<string> InputChannel { get; }
        }
    }
}