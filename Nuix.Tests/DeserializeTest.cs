using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public abstract partial class NuixStepTestBase<TStep, TOutput>
    {
        public class DeserializeUnitTest : DeserializeCase
        {
            /// <inheritdoc />
            public DeserializeUnitTest(string name, string yaml, TOutput expectedOutput, IReadOnlyCollection<string> valuesToLog, params string[] expectedLoggedValues) :
                base(name, yaml, expectedOutput, expectedLoggedValues) => SetupRunner(valuesToLog);

            /// <inheritdoc />
            public DeserializeUnitTest(string name, string yaml, Unit _, IReadOnlyCollection<string> valuesToLog, params string[] expectedLoggedValues) :
                base(name, yaml, _, expectedLoggedValues) =>
                SetupRunner(valuesToLog);

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
                        It.IsAny<IErrorHandler>(),
                        It.IsAny<IEnumerable<string>>(),
                        Encoding.UTF8,
                        It.IsAny<CancellationToken>()
                        
                        
                        ))
                    .Callback<string, ILogger, IErrorHandler, IEnumerable<string>, Encoding,CancellationToken>((s, logger, arg3, arg4, e, ct) =>
                    {
                        foreach (var val in valuesToLog)
                        {
                            logger.LogInformation(val);
                        }

                    })
                    .ReturnsAsync(Unit.Default));
            }
        }
    }
}
