using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
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
            var stepTest = new NuixStepTestBase<NuixCreateCase, Unit>.IntegrationTestCase(
                    // Testing this workflow: https://docs.reductech.io/edr/examples/create-ingest-export.html
                    "Sequence - create, add, ocr, search&tag, report, export",
                    new Sequence<Unit>
                    {
                        InitialSteps = new[]
                        {
                            new DeleteItem { Path = Constant(CasePath) },
                            new DeleteItem { Path = Constant(ReportPath) },
                            new DeleteItem { Path = Constant(ExportPath) },
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
                            new AssertTrue
                            {
                                Boolean = new NuixDoesCaseExist
                                {
                                    CasePath = Constant(CasePath)
                                }
                            },
                            new NuixOpenCase { CasePath = Constant(CasePath) },
                            // Add loose items
                            new NuixAddItem
                            {
                                Custodian  = Constant("EDRM Micro"),
                                Paths      = Array(DataPath),
                                FolderName = Constant("INT01B0001"),
                                MimeTypeSettings = Array(
                                    Entity.Create(("mime_type", "text/plain"), ("enabled", "true")),
                                    Entity.Create(
                                        ("mime_type", "application/pdf"),
                                        ("enabled", "true")
                                    )
                                )
                            },
                            AssertCount(186, "custodian:\"EDRM Micro\""), AssertCount(2, "*.txt"),
                            // Add concordance file
                            new NuixAddConcordance
                            {
                                ConcordanceProfileName = Constant("IntegrationTestProfile"),
                                ConcordanceDateFormat  = Constant("yyyy-MM-dd'T'HH:mm:ss.SSSZ"),
                                FilePath               = ConcordancePath,
                                Custodian              = Constant("Reductech EDR"),
                                FolderName             = Constant("INT01B0002")
                            },
                            AssertCount(3, "custodian:\"Reductech EDR\""), AssertCount(3, "*.txt"),
                            // OCR the data
                            AssertCount(0, "transparency"),
                            new NuixPerformOCR
                            {
                                SearchTerm     = Constant("mime-type:image/jpeg"),
                                OCRProfileName = Constant("Default")
                            },
                            AssertCount(2, "transparency"),
                            // Run a search and tag
                            new ForEach<Entity>
                            {
                                Array = Array(
                                    Entity.Create(("SearchTerm", "*.jpg"), ("Tag", "image")),
                                    Entity.Create(("SearchTerm", "blue"),  ("Tag", "colour"))
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
                                        Entity   = GetEntityVariable,
                                        Property = Constant("Tag")
                                    }
                                }
                            },
                            // Create an item set from the tagged items
                            new NuixAddToItemSet
                            {
                                SearchTerm  = Constant("tag:*"),
                                ItemSetName = Constant("TaggedItems")
                            },
                            AssertCount(13, "item-set:TaggedItems"),
                            // Create a production set from the tagged items
                            new NuixAddToProductionSet
                            {
                                SearchTerm            = Constant("item-set:TaggedItems"),
                                ProductionSetName     = Constant("ExportProduction"),
                                ProductionProfilePath = TestProductionProfilePath
                            },
                            AssertCount(13, "production-set:ExportProduction"),
                            new NuixRemoveFromProductionSet
                            {
                                ProductionSetName = Constant("ExportProduction"),
                                SearchTerm        = Constant("name:IMG_17*")
                            },
                            AssertCount(11, "production-set:ExportProduction"),
                            // Write out a file type report
                            new CreateDirectory { Path = Constant(ReportPath) },
                            new FileWrite
                            {
                                Path = new PathCombine
                                {
                                    Paths = Array(ReportPath, "file-types.txt")
                                },
                                Stream = new NuixCreateReport()
                            },
                            AssertFileContains(ReportPath, "file-types.txt", "*	kind	*	189"),
                            AssertFileContains(
                                ReportPath,
                                "file-types.txt",
                                "EDRM Micro	kind	*	186"
                            ),
                            AssertFileContains(
                                ReportPath,
                                "file-types.txt",
                                "Reductech EDR	kind	*	3"
                            ),
                            // Write out a term list
                            new FileWrite
                            {
                                Path = new PathCombine
                                {
                                    Paths = Array(ReportPath, "terms-list.txt")
                                },
                                Stream = new NuixCreateTermList()
                            },
                            AssertFileContains(ReportPath, "terms-list.txt", "garnethill    222"),
                            AssertFileContains(ReportPath, "terms-list.txt", "email	253"),
                            // Export concordance from the production set
                            new NuixExportConcordance
                            {
                                ProductionSetName = Constant("ExportProduction"),
                                ExportPath        = Constant(ExportPath)
                            },
                            AssertFileContains(
                                ExportPath,
                                "loadfile.dat",
                                "25858867143438cf972761a1e45249fa"
                            ),
                            AssertFileContains(
                                ExportPath,
                                "loadfile.dat",
                                "6b661c59b9cc39b84832e3b7ebee6e93"
                            ),
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
                )
                .WithSettings(
                    NuixSettingsList.First()
                ); // Only run these tests on the latest version of nuix that we support.

            stepTest.OutputLogLevel1 = LogLevel.Debug;
            yield return stepTest;
        }
    }
}

}
