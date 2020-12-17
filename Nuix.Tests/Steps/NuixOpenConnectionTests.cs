using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Moq;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixOpenConnectionTests : StepTestBase<NuixOpenConnection, Unit>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public NuixOpenConnectionTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                var mock = new ExternalProcessMock.ProcessReferenceMock();

                var case1 = new StepCase("Open New Connection", new NuixOpenConnection(), Unit.Default)
                    {
                        IgnoreFinalState = true
                    }
                    .WithSettings(new NuixSettings(true, "TestPath", new Version(1,0), Constants.AllNuixFeatures ))
                    .WithFileSystemAction(x=>x.Setup(f=>f.DoesFileExist(It.IsAny<string>())).Returns(true))
                    .WithExternalProcessAction(x=>x.Setup(a=>
                        a.StartExternalProcess("TestPath", It.IsAny<IEnumerable<string>>(), It.IsAny<Encoding>()))
                        .Returns(Result.Success<IExternalProcessReference, IErrorBuilder>(mock)));

                yield return case1;
            }
        }

        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                yield break;
            }
        }
    }
}