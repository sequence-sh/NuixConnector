using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Sdk;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

    internal class ExternalProcessMock : IExternalProcessRunner
    {
        public ExternalProcessMock(int expectedTimesStarted)
        {
            ExpectedTimesStarted = expectedTimesStarted;
        }

        public int ExpectedTimesStarted { get; }

        public int TimesStarted { get; private set; } = 0;

        /// <inheritdoc />
        public async Task<Result<Unit, IErrorBuilder>> RunExternalProcess(string processPath, ILogger logger, IErrorHandler errorHandler, IEnumerable<string> arguments,
            Encoding encoding)
        {
            await Task.CompletedTask;

            throw new XunitException("nuix processes should not RunExternalProcess");
        }

        /// <inheritdoc />
        public Result<IExternalProcessReference, IErrorBuilder> StartExternalProcess(string processPath, IEnumerable<string> arguments, Encoding encoding)
        {
            TimesStarted++;
            if(TimesStarted > ExpectedTimesStarted)
                throw new XunitException($"Should only start external process {ExpectedTimesStarted} times");

            processPath.Should().Contain("nuix_console.exe");

            encoding.Should().Be(Encoding.UTF8);

            arguments.Should().BeEquivalentTo(ExpectedArguments);

            return new ProcessReferenceMock();
        }

        private static readonly List<string> ExpectedArguments = new List<string>()
        {
            "--licenseSourceType",
            "useDongle",
            "C:/Script"
        };

        private class ProcessReferenceMock : IExternalProcessReference
        {
            public ProcessReferenceMock()
            {
                InputStream = new StreamWriter(InnerInputStream);

                Run();
            }



            /// <inheritdoc />
            public void Dispose()
            {
                IsDisposed = true;
            }

            public bool IsDisposed { get; private set; }

            /// <inheritdoc />
            public void WaitForExit(int? milliseconds) => throw new XunitException("Should not wait for exit");

            public async Task Run()
            {
                var streamReader = new StreamReader(InnerInputStream);

                while (true)
                {
                    var line = await streamReader.ReadLineAsync();

                    if(IsDisposed)
                        throw new ObjectDisposedException("Process Reference is disposed");
                }
            }

            /// <inheritdoc />
            public IStreamReader<(string line, StreamSource source)> OutputStream { get; }

            /// <inheritdoc />
            public StreamWriter InputStream { get; }

            private Stream InnerInputStream { get; } = new MemoryStream();
        }
    }


    public abstract partial class NuixStepTestBase<TStep, TOutput>
    {
        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases { get { yield break; } }

        public class UnitTest : StepCase
        {

            public UnitTest(string name,
                Sequence sequence,
                IReadOnlyCollection<string> valuesToLog,
                IEnumerable<(string key, string value)> expectedExtraArgs,
                params string[] expectedLogValues)
                : base(name, sequence, Maybe<TOutput>.None, expectedLogValues)
            {
                SetupRunner(valuesToLog, expectedExtraArgs.ToList());
            }

            public UnitTest(string name,
                TStep step,
                TOutput expectedOutput,
                IReadOnlyCollection<string> valuesToLog,
                IEnumerable<(string key, string value)> expectedExtraArgs,
                params string[] expectedLogValues)
                : base(name, step, expectedOutput, expectedLogValues)
            {
                SetupRunner(valuesToLog, expectedExtraArgs.ToList());
            }

            private void SetupRunner(IEnumerable<string> valuesToLog, IReadOnlyList<(string key, string value)> expectedArgPairs)
            {
                //AddFileSystemAction(x => x.Setup(y => y.WriteFileAsync(
                //     It.IsRegex(@".*\.rb"),
                //     It.Is<Stream>(s=> ValidateRubyScript(s, expectedArgPairs)),
                //     It.IsAny<CancellationToken>()
                // )).ReturnsAsync(Unit.Default));


                AddExternalProcessRunnerAction(externalProcessRunner =>
                    externalProcessRunner
                        .Setup(y => y
                                .RunExternalProcess(
                        It.IsAny<string>(),
                        It.IsAny<ILogger>(),
                        It.IsAny<IErrorHandler>(),
                        It.Is<IEnumerable<string>>(ie => AreExternalArgumentsCorrect(ie, expectedArgPairs)),
                        Encoding.UTF8))
                    .Callback<string, ILogger, IErrorHandler, IEnumerable<string>, Encoding>((s, logger, arg3, arg4, e) =>
                    {
                        foreach (var val in valuesToLog)
                        {
                            logger.LogInformation(val);
                        }

                    })
                    .ReturnsAsync(Unit.Default));
            }

            private static bool ValidateRubyScript(Stream stream, IEnumerable<(string key, string value)> expectedArgPairs)
            {
                var reader = new StreamReader(stream);
                var text = reader.ReadToEnd();
                text.Should().NotBeNull();

                text.Should().Contain("require 'optparse'"); //very shallow testing that this is actually a ruby script

                foreach (var (key, _) in expectedArgPairs)
                {
                    text.Should().Contain(key);
                }

                return true;
            }

            private static bool AreExternalArgumentsCorrect(IEnumerable<string> externalProcessArgs, IReadOnlyList<(string key, string value)> expectedArgPairs)
            {
                var list = externalProcessArgs.ToList();
                var extraArgs = list.Skip(3).ToList();
                list[0].Should().Be("-licencesourcetype");
                list[1].Should().Be("dongle");
                list[2].Should().Match("*.rb");

                var realArgPairs = new List<(string key, string value)>();

                for (var i = 0; i < extraArgs.Count() - 1; i+=2)
                {
                    var key = extraArgs[i];
                    var value = extraArgs[i + 1];

                    realArgPairs.Add((key, value));
                }

                realArgPairs.Select(x => x.key).Should().BeEquivalentTo(expectedArgPairs.Select(x => "--" + x.key));

                foreach (var ((key, realValue), (_, expectedValue)) in realArgPairs.Zip(expectedArgPairs))
                {
                    realValue.Should().Contain(expectedValue, $"values of '{key}' should match");
                }


                return true;
            }
        }
    }
}
