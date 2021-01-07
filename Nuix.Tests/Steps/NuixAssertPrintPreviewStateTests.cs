using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public class NuixAssertPrintPreviewStateTests : NuixStepTestBase<NuixAssertPrintPreviewState, Unit>
{
    /// <inheritdoc />
    public NuixAssertPrintPreviewStateTests(ITestOutputHelper testOutputHelper) : base(
        testOutputHelper
    ) { }

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
                    new ExternalProcessAction(
                        new ConnectionCommand
                        {
                            Command = "GetPrintPreviewState",
                            Arguments = new Dictionary<string, object>
                            {
                                {
                                    nameof(NuixAssertPrintPreviewState.CasePath),
                                    CasePathString
                                },
                                {
                                    nameof(NuixAssertPrintPreviewState.ProductionSetName),
                                    "Production Set"
                                },
                                {
                                    nameof(NuixAssertPrintPreviewState.ExpectedState),
                                    PrintPreviewState.All.ToString()
                                }
                            }
                        },
                        new ConnectionOutput
                        {
                            Result = new ConnectionOutputResult { Data = null }
                        }
                    )
                }
            ).WithSettings(UnitTestSettings);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases { get { yield break; } }
}

}
