namespace Reductech.Sequence.Connectors.Nuix.Tests.Steps
{

public partial class
    NuixGeneratePrintPreviewsTests : NuixStepTestBase<NuixGeneratePrintPreviews, Unit>
{
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
                SetupCase,
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
                CleanupCase
            );
        }
    }
}

}
