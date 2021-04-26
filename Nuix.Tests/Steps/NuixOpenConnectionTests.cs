using System;
using System.Collections.Generic;
using System.Text;
using CSharpFunctionalExtensions;
using Moq;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixOpenConnectionTests : StepTestBase<NuixOpenConnection, Unit>
{
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var mock = new ExternalProcessMock.ProcessReferenceMock();

            var case1 = new StepCase("Open New Connection", new NuixOpenConnection(), Unit.Default)
                {
                    IgnoreFinalState = true
                }
                .WithSettings(
                    NuixSettings.CreateSettings(
                        "TestPath",
                        new Version(1, 0),
                        true,
                        Constants.AllNuixFeatures
                    )
                )
                .WithScriptExists()
                .WithExternalProcessAction(
                    x => x.Setup(
                            a =>
                                a.StartExternalProcess(
                                    "TestPath",
                                    It.IsAny<IEnumerable<string>>(),
                                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                                    It.IsAny<Encoding>(),
                                    It.IsAny<IStateMonad>(),
                                    It.IsAny<IStep>()
                                )
                        )
                        .Returns(Result.Success<IExternalProcessReference, IErrorBuilder>(mock))
                );

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
