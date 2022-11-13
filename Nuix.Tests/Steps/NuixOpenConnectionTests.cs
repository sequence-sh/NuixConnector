using System.Text;
using CSharpFunctionalExtensions;
using Moq;
using Sequence.Core.ExternalProcesses;
using Sequence.Core.Internal.Errors;
using Sequence.Core.TestHarness;

namespace Sequence.Connectors.Nuix.Tests.Steps;

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
                .WithStepFactoryStore(
                    SettingsHelpers.CreateStepFactoryStore(
                        new NuixSettings(
                            "TestPath",
                            new Version(1, 0),
                            true,
                            Constants.AllNuixFeatures
                        )
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
