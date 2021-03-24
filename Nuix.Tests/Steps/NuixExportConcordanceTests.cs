using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

using Reductech.EDR.Core;

public partial class NuixExportConcordanceTests : NuixStepTestBase<NuixExportConcordance, Unit>
{
    private readonly string _concordancePath1 = Path.Combine(GeneralDataFolder, "Concordance1");
    private readonly string _concordancePath2 = Path.Combine(GeneralDataFolder, "Concordance2");

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Export Concordance",
                DeleteCaseFolder,
                new DeleteItem { Path = Constant(_concordancePath1) },
                new DeleteItem { Path = Constant(_concordancePath2) },
                CreateCase,
                AddData,
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("charm"),
                    ProductionSetName     = Constant("charmset"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                new NuixExportConcordance
                {
                    ProductionSetName = Constant("charmset"),
                    ExportPath        = Constant(_concordancePath1)
                },
                AssertFileContains(_concordancePath1, "loadfile.dat", "DOCID"),
                AssertFileContains(
                    _concordancePath1,
                    "TEXT/000/000/DOC-000000001.txt",
                    "Visible, invisible"
                ),
                new NuixAddToProductionSet
                {
                    SearchTerm        = Constant("\"and\""),
                    ProductionSetName = Constant("conjunction"),
                    NumberingOptions = Constant(
                        Entity.Create(
                            ("createProductionSet", false),
                            ("prefix", "ABC"),
                            ("documentId", Entity.Create(("startAt", 5), ("minWidth", 3)))
                        )
                    )
                },
                new NuixExportConcordance
                {
                    ExportPath        = Constant(_concordancePath2),
                    ProductionSetName = Constant("conjunction"),
                    ExportOptions = Constant(
                        Entity.Create(
                            ("native",
                             Entity.Create(("path", "NATIVE"), ("naming", "document_id"))),
                            ("text", Entity.Create(("path", "TEXT"), ("naming", "document_id")))
                        )
                    ),
                    NumberingOptions = Constant(
                        Entity.Create(
                            ("createProductionSet", false),
                            ("prefix", "ABC"),
                            ("documentId", Entity.Create(("startAt", 5), ("minWidth", 3)))
                        )
                    ),
                    LoadFileType           = Constant(LoadFileType.Concordance),
                    LoadFileOptions        = Constant(Entity.Create(("loadFileEntryLimit", 5000))),
                    TraversalStrategy      = Constant(ExportTraversalStrategy.Items),
                    Deduplication          = Constant(ExportDeduplication.None),
                    SortOrder              = Constant(ExportSortOrder.DocumentId),
                    SkipSlipsheetedNatives = Constant(true),
                    FailedItemsTag         = Constant("FailedExport")
                },
                new AssertTrue
                {
                    Boolean = new FileExists
                    {
                        Path = Constant(
                            Path.Combine(_concordancePath2, "NATIVE/ABC005.txt")
                        )
                    }
                },
                new AssertTrue
                {
                    Boolean = new FileExists
                    {
                        Path = Constant(Path.Combine(_concordancePath2, "TEXT/ABC005.txt"))
                    }
                },
                AssertFileContains(_concordancePath2, "loadfile.dat", "þNATIVE\\ABC005.txtþ"),
                new NuixCloseConnection(),
                new DeleteItem { Path = Constant(_concordancePath1) },
                new DeleteItem { Path = Constant(_concordancePath2) },
                DeleteCaseFolder
            );

            yield return new NuixIntegrationTestCase(
                "Export Concordance Errors",
                DeleteCaseFolder,
                CreateCase,
                AddData,
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("charm"),
                    ProductionSetName     = Constant("charmset"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                new NuixAddToProductionSet
                {
                    SearchTerm        = Constant("\"and\""),
                    ProductionSetName = Constant("conjunction")
                },
                // Fail because no production profile or export options defined
                new AssertError
                {
                    Step = new NuixExportConcordance
                    {
                        ProductionSetName = Constant("conjunction"),
                        ExportPath        = Constant("TestPath")
                    }
                },
                // Fail because production set does not exist
                new AssertError
                {
                    Step = new NuixExportConcordance
                    {
                        ProductionSetName = Constant("DoesNotExist"),
                        ExportPath        = Constant("TestPath")
                    }
                },
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
