using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class
    NuixAssertPrintPreviewStateTests : NuixStepTestBase<NuixAssertPrintPreviewState, Unit>
{
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new NuixStepCase(
                    "Check State All",
                    new NuixAssertPrintPreviewState
                    {
                        CasePath          = CasePath,
                        ProductionSetName = Constant("Production Set"),
                        ExpectedState     = Constant(PrintPreviewState.All)
                    },
                    Unit.Default,
                    new List<ExternalProcessAction>
                    {
                        new(
                            new ConnectionCommand
                            {
                                Command = "GetPrintPreviewState",
                                Arguments = new Dictionary<string, object>
                                {
                                    {
                                        nameof(NuixAssertPrintPreviewState.ProductionSetName),
                                        "Production Set"
                                    },
                                    {
                                        nameof(NuixAssertPrintPreviewState.ExpectedState),
                                        PrintPreviewState.All.ToString()
                                    },
                                    {
                                        nameof(RubyCaseScriptStepBase<bool>.CasePath),
                                        CasePathString
                                    }
                                }
                            },
                            new ConnectionOutput
                            {
                                Result = new ConnectionOutputResult { Data = null }
                            }
                        )
                    }
                )
                .WithStepFactoryStore(UnitTestSettings);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases { get { yield break; } }
}

}
