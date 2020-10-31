using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public abstract partial class NuixStepTestBase<TStep, TOutput>
    {
        public class DeserializeUnitTest : DeserializeCase
        {
            /// <inheritdoc />
            public DeserializeUnitTest(string name, string yaml, TOutput expectedOutput, IReadOnlyCollection<string> valuesToLog, params string[] expectedLoggedValues) : base(name, yaml, expectedOutput, expectedLoggedValues)
            {
                SetupRunner(valuesToLog);
            }

            /// <inheritdoc />
            public DeserializeUnitTest(string name, string yaml, Unit _, IReadOnlyCollection<string> valuesToLog, params string[] expectedLoggedValues) : base(name, yaml, _, expectedLoggedValues)
            {
                SetupRunner(valuesToLog);
            }

            private void SetupRunner(IEnumerable<string> valuesToLog)
            {
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
