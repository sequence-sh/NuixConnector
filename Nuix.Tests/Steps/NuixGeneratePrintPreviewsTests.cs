using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public class NuixGeneratePrintPreviewsTests : NuixStepTestBase<NuixGeneratePrintPreviews, Unit>
{
    /// <inheritdoc />
    public NuixGeneratePrintPreviewsTests(ITestOutputHelper testOutputHelper) : base(
        testOutputHelper
    ) { }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get { yield break; }
    }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Generate Print Previews",
                DeleteCaseFolder,
                CreateCase,
                AddData,
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("*.txt"),
                    ProductionSetName     = Constant("prodSet"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                AssertCount(2, "production-set:prodSet"),
                new NuixAssertPrintPreviewState
                {
                    ProductionSetName = Constant("prodSet"),
                    ExpectedState     = Constant(PrintPreviewState.None)
                },
                new NuixGeneratePrintPreviews { ProductionSetName = Constant("prodSet") },
                new NuixAssertPrintPreviewState
                {
                    ProductionSetName = Constant("prodSet"),
                    ExpectedState     = Constant(PrintPreviewState.All)
                },
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
