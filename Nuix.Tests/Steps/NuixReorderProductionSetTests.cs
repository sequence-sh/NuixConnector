namespace Sequence.Connectors.Nuix.Tests.Steps;

public partial class
    NuixReorderProductionSetTests : NuixStepTestBase<NuixReorderProductionSet, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases { get { yield break; } }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Reorder From Production Set",
                SetupCase,
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("*.txt"),
                    ProductionSetName     = Constant("fullset"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                new NuixReorderProductionSet
                {
                    ProductionSetName = Constant("fullset"),
                    SortOrder         = Constant(ItemSortOrder.TopLevelItemDate)
                },
                AssertCount(2, "production-set:fullset"),
                CleanupCase
            );
        }
    }
}
