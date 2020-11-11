using System.Collections.Generic;
using System.IO;
using System.Threading;
using CSharpFunctionalExtensions;
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

            public UnitTest(string name, Sequence sequence, IReadOnlyCollection<string> valuesToLog, params string[] expectedLogValues)
                : base(name, sequence, Maybe<TOutput>.None, expectedLogValues)
            {
                SetupRunner(valuesToLog);
            }

            public UnitTest(string name, TStep step, TOutput expectedOutput, IReadOnlyCollection<string> valuesToLog, params string[] expectedLogValues)
                : base(name, step, expectedOutput, expectedLogValues)
            {
                SetupRunner(valuesToLog);
            }

            private void SetupRunner(IEnumerable<string> valuesToLog)
            {
                AddFileSystemAction(x => x.Setup(y => y.WriteFileAsync(
                     It.IsRegex(@".*\.rb"),
                     It.IsAny<MemoryStream>(),
                     It.IsAny<CancellationToken>()
                 )).ReturnsAsync(Unit.Default));


                AddExternalProcessRunnerAction(externalProcessRunner =>
                    externalProcessRunner.Setup(y => y.RunExternalProcess(It.IsAny<string>(),
                        It.IsAny<ILogger>(),
                        It.IsAny<IErrorHandler>(), It.IsAny<IEnumerable<string>>()))
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


        }
    }
}
