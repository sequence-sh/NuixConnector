namespace Reductech.Sequence.Connectors.Nuix.Tests.Steps;

public partial class
    NuixRemoveFromProductionSetTests : NuixStepTestBase<NuixRemoveFromProductionSet, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Remove From Production Set",
                SetupCase,
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("*.txt"),
                    ProductionSetName     = Constant("fullset"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                new NuixRemoveFromProductionSet
                {
                    SearchTerm = Constant("Charm"), ProductionSetName = Constant("fullset")
                },
                AssertCount(1, "production-set:fullset"),
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("\"and\""),
                    ProductionSetName     = Constant("conjunction"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                new NuixRemoveFromProductionSet
                {
                    ProductionSetName = Constant("conjunction"),
                    SearchTerm        = Constant("jellyfish"),
                    SearchOptions     = Constant(Entity.Create(("defaultFields", new[] { "name" })))
                },
                AssertCount(1, "production-set:conjunction"),
                new NuixRemoveFromProductionSet { ProductionSetName = Constant("conjunction") },
                AssertCount(0, "production-set:conjunction"),
                CleanupCase
            );
        }
    }
}
