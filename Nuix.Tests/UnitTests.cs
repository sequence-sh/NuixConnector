using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
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

            private void SetupRunner(IEnumerable<string> valuesToLog, IEnumerable<(string key, string value)> expectedArgPairs)
            {
                AddFileSystemAction(x => x.Setup(y => y.WriteFileAsync(
                     It.IsRegex(@".*\.rb"),
                     It.Is<Stream>(s=> ValidateRubyScript(s)),
                     It.IsAny<CancellationToken>()
                 )).ReturnsAsync(Unit.Default));


                AddExternalProcessRunnerAction(externalProcessRunner =>
                    externalProcessRunner.Setup(y => y.RunExternalProcess(It.IsAny<string>(),
                        It.IsAny<ILogger>(),
                        It.IsAny<IErrorHandler>(), It.Is<IEnumerable<string>>(ie=> AreExternalArgumentsCorrect(ie, expectedArgPairs))))
                    .Callback<string, ILogger, IErrorHandler, IEnumerable<string>>((s, logger, arg3, arg4) =>
                    {
                        foreach (var val in valuesToLog)
                        {
                            logger.LogInformation(val);
                        }

                        logger.LogInformation(ScriptGenerator.UnitSuccessToken);
                    })
                    .ReturnsAsync(Unit.Default));
            }

            private static bool ValidateRubyScript(Stream stream)
            {
                var reader = new StreamReader(stream);
                var text = reader.ReadToEnd();
                text.Should().NotBeNull();
                text.Should().Contain("require 'optparse'"); //very shallow testing that this is actually a ruby script

                return true;
            }

            private static bool AreExternalArgumentsCorrect(IEnumerable<string> externalProcessArgs, IEnumerable<(string key, string value)> expectedArgPairs)
            {
                var list = externalProcessArgs.ToList();
                var extraArgs = list.Skip(3);
                list[0].Should().Be("-licencesourcetype");
                list[1].Should().Be("dongle");
                list[2].Should().Match("*.rb");

                var expectedExtraArgs = expectedArgPairs.SelectMany(x => new[] {$"-{x.key}", x.value});

                extraArgs.Should().BeEquivalentTo(expectedExtraArgs);

                return true;
            }
        }
    }
}
