namespace Reductech.Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixAssignCustodianTests : NuixStepTestBase<NuixAssignCustodian, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Assign Custodians",
                SetupCase,
                AssertCount(0, "custodian:\"Jason\""),
                new NuixAssignCustodian
                {
                    Custodian = Constant("Jason"), SearchTerm = Constant("charm")
                },
                AssertCount(1, "custodian:\"Jason\""),
                new NuixAssignCustodian
                {
                    Custodian     = Constant("John"),
                    SearchTerm    = Constant("*.txt"),
                    SortSearch    = Constant(true),
                    SearchOptions = Constant(Entity.Create(("limit", 1)))
                },
                AssertCount(1, "custodian:\"John\""),
                CleanupCase
            );
        }
    }
}
