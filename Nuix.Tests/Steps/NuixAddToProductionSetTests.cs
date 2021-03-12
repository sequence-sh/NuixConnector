using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixAddToProductionSetTests : NuixStepTestBase<NuixAddToProductionSet, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Add to production set",
                DeleteCaseFolder,
                CreateCase,
                AddData,
                new NuixAddToProductionSet
                {
                    SearchTerm        = Constant("terrible"),
                    ProductionSetName = Constant("TerribleSet"),
                    ItemSortOrder     = Constant(ItemSortOrder.DocumentId),
                    ImagingOptions =
                        Constant(
                            Core.Entity.Create(
                                ("imageExcelSpreadsheets", true),
                                ("slipSheetContainers", true)
                            )
                        ),
                    NumberingOptions =
                        Constant(
                            Core.Entity.Create(
                                ("createProductionSet", false),
                                ("prefix", "ABC"),
                                ("documentId", Core.Entity.Create(("startAt", 1)))
                            )
                        ),
                    StampingOptions =
                        Constant(
                            Core.Entity.Create(
                                ("footerCentre", Core.Entity.Create(("type", "document_number")))
                            )
                        ),
                    TextOptions = Constant(
                        Core.Entity.Create(("lineSeparator", "\\n"), ("encoding", "UTF-8"))
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
                    SearchOptions            = Constant(Core.Entity.Create(("limit", 1)))
                },
                AssertCount(1, "production-set:conjunction"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
