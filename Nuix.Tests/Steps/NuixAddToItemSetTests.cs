namespace Reductech.Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixAddToItemSetTests : NuixStepTestBase<NuixAddToItemSet, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Add To Item Set",
                SetupCase,
                new NuixAddToItemSet
                {
                    SearchTerm = Constant("charm"), ItemSetName = Constant("charmset")
                },
                AssertCount(1, "item-set:charmset"),
                new NuixAddToItemSet
                {
                    SearchTerm    = Constant("\"and\""),
                    ItemSetName   = Constant("conjunction"),
                    SortSearch    = Constant(true),
                    SearchOptions = Constant(Entity.Create(("limit", 1)))
                },
                AssertCount(1, "item-set:conjunction"),
                CleanupCase
            );
        }
    }
}
