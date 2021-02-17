using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

[Collection("RequiresNuixLicense")]
public partial class IntegrationShortTests
{
    private const string CasePath = @"D:\Shares\Cases\Nuix\IntegrationShort\Case";
    private const string DataPath = @"D:\Shares\Cases\Nuix\IntegrationShort\Data";
    private const string ReportPath = @"D:\Shares\Cases\Nuix\IntegrationShort\Reports";
    private const string ExportPath = @"D:\Shares\Cases\Nuix\IntegrationShort\Export";

    [AutoTheory.GenerateAsyncTheory("NuixIntegration", Category = "IntegrationShort")]

    public IEnumerable<NuixStepTestBase<NuixCreateCase, Unit>.IntegrationTestCase>
        IntegrationTestCases
    {
        get
        {
            yield return new NuixStepTestBase<NuixCreateCase, Unit>.IntegrationTestCase(
                // Testing this workflow: https://docs.reductech.io/edr/examples/create-ingest-export.html
                "Sequence - create, add, ocr, search&tag, report, export",
                new Sequence<Unit>
                {
                    InitialSteps = new[]
                    {
                        new DeleteItem { Path = Constant(CasePath) },
                        new AssertTrue
                        {
                            Boolean = new Not
                            {
                                Boolean = new NuixDoesCaseExist
                                {
                                    CasePath = Constant(CasePath)
                                }
                            }
                        },
                        // Create and open a case
                        new NuixCreateCase
                        {
                            CaseName     = Constant("IntegrationShort"),
                            CasePath     = Constant(CasePath),
                            Investigator = Constant("InvestigatorA")
                        },
                        new NuixOpenCase() { CasePath = Constant(CasePath) },
                        // Add loose items
                        new NuixAddItem
                        {
                            Custodian  = Constant("EDRM Micro"),
                            Paths      = Array(DataPath),
                            FolderName = Constant("INT01B0001"),
                            MimeTypeSettings = Array(
                                CreateEntity(("mime_type", "text/plain"),      ("enabled", "true")),
                                CreateEntity(("mime_type", "application/pdf"), ("enabled", "true"))
                            )
                        },
                        //AssertCount(2, "*.txt"),
                        // Add concordance file
                        new NuixAddConcordance
                        {
                            ConcordanceProfileName = Constant("IntegrationTestProfile"),
                            ConcordanceDateFormat  = Constant("yyyy-MM-dd'T'HH:mm:ss.SSSZ"),
                            FilePath               = ConcordancePath,
                            Custodian              = Constant("Reductech EDR"),
                            FolderName             = Constant("INT01B0002")
                        },
                        //AssertCount(3, "*.txt"),
                        // OCR the data
                        //AssertCount(1, "digital debris"),
                        new NuixPerformOCR { OCRProfileName = Constant("Default") },
                        //AssertCount(1, "digital debris"),
                        // Run a search and tag
                        new ForEach<Entity>
                        {
                            Array = Array(
                                CreateEntity(("SearchTerm", "*.jpg"), ("Tag", "image")),
                                CreateEntity(("SearchTerm", "*.doc"), ("Tag", "document")),
                                CreateEntity(("SearchTerm", "red"),   ("Tag", "colour"))
                            ),
                            Action = new NuixSearchAndTag
                            {
                                SearchTerm = new EntityGetValue
                                {
                                    Entity   = GetEntityVariable,
                                    Property = Constant("SearchTerm")
                                },
                                Tag = new EntityGetValue
                                {
                                    Entity = GetEntityVariable, Property = Constant("Tag")
                                }
                            }
                        },
                        //AssertCount(1, "tag:charm"),
                        // Create an item set from the tagged items
                        new NuixAddToItemSet
                        {
                            SearchTerm  = Constant("tag:*"),
                            ItemSetName = Constant("TaggedItems")
                        },
                        //AssertCount(1, "item-set:TaggedItems"),
                        // Create a production set from the tagged items
                        new NuixAddToProductionSet
                        {
                            SearchTerm            = Constant("item-set:TaggedItems"),
                            ProductionSetName     = Constant("ProductionSet1"),
                            ProductionProfilePath = TestProductionProfilePath
                        },
                        //AssertCount(1, "production-set:ProductionSet1"),
                        // Write out a file type report
                        new FileWrite
                        {
                            Path = new PathCombine
                            {
                                Paths = Array(ReportPath, "file-types.txt")
                            },
                            Stream = new NuixCreateReport()
                        },
                        //AssertFileContains(ReportPath, "file-types.txt", ""),
                        // Export concordance from the production set
                        new NuixExportConcordance
                        {
                            ProductionSetName = Constant("ProductionSet1"),
                            ExportPath        = Constant(ExportPath)
                        },
                        AssertFileContains(ExportPath, "loadfile.dat", "DOCID"),
                        //AssertFileContains(
                        //    ExportPath,
                        //    "TEXT/000/000/DOC-000000001.txt",
                        //    "Visible, invisible"
                        //),
                        new NuixCloseConnection(),
                        // clean up
                        new DeleteItem { Path = Constant(CasePath) },
                        new DeleteItem { Path = Constant(ReportPath) },
                        new DeleteItem { Path = Constant(ExportPath) },
                        new ForEach<StringStream>
                        {
                            Array = Array(CasePath, ReportPath, ExportPath),
                            Action = AssertDirectoryDoesNotExist(
                                GetVariable<StringStream>(VariableName.Entity)
                            )
                        }
                    }
                }
            ).WithSettings(
                NuixSettingsList.First()
            ); // Only run these tests on the latest version of nuix that we support.
        }
    }
}

}
