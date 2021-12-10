namespace Reductech.EDR.Connectors.Nuix.Tests.Steps;

public partial class NuixAddToProductionSetTests : NuixStepTestBase<NuixAddToProductionSet, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Add to production set",
                SetupCase,
                new NuixAddToProductionSet
                {
                    SearchTerm        = Constant("terrible"),
                    ProductionSetName = Constant("TerribleSet"),
                    ItemSortOrder     = Constant(ItemSortOrder.DocumentId),
                    ImagingOptions =
                        Constant(
                            Entity.Create(
                                ("imageExcelSpreadsheets", true),
                                ("slipSheetContainers", true)
                            )
                        ),
                    NumberingOptions =
                        Constant(
                            Entity.Create(
                                ("createProductionSet", false),
                                ("prefix", "ABC"),
                                ("documentId", Entity.Create(("startAt", 1)))
                            )
                        ),
                    StampingOptions =
                        Constant(
                            Entity.Create(
                                ("footerCentre", Entity.Create(("type", "document_number")))
                            )
                        ),
                    TextOptions = Constant(
                        Entity.Create(("lineSeparator", "\\n"), ("encoding", "UTF-8"))
                    )
                },
                AssertCount(1, "production-set:TerribleSet"),
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("charm"),
                    ProductionSetName     = Constant("charmset"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                AssertCount(1, "production-set:charmset"),
                new NuixAddToProductionSet
                {
                    SearchTerm               = Constant("\"and\""),
                    ProductionSetName        = Constant("conjunction"),
                    ProductionSetDescription = Constant("description"),
                    ProductionProfilePath    = TestProductionProfilePath,
                    SortSearch               = Constant(true),
                    SearchOptions            = Constant(Entity.Create(("limit", 1)))
                },
                AssertCount(1, "production-set:conjunction"),
                CleanupCase
            );
        }
    }
}
