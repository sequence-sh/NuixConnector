using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps;

public partial class NuixAddItemTests : NuixStepTestBase<NuixAddItem, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new NuixStepCase(
                    "Add item",
                    new NuixAddItem
                    {
                        Custodian          = Constant("Mark"),
                        Paths              = DataPaths,
                        Container          = Constant("New Folder"),
                        ProcessingSettings = Constant(Entity.Create(("Foo", "Bar"))),
                        CasePath           = CasePath
                    },
                    Unit.Default,
                    new List<ExternalProcessAction>
                    {
                        new(
                            new ConnectionCommand
                            {
                                Command = "AddToCase",
                                Arguments = new Dictionary<string, object>
                                {
                                    {
                                        nameof(RubyCaseScriptStepBase<SCLBool>.CasePath),
                                        CasePathString
                                    },
                                    { nameof(NuixAddItem.Container), "New Folder" },
                                    { nameof(NuixAddItem.Custodian), "Mark" },
                                    {
                                        nameof(NuixAddItem.Paths),
                                        new List<string> { DataPathString }
                                    },
                                    {
                                        nameof(NuixAddItem.ProcessingSettings),
                                        Entity.Create(("Foo", "Bar"))
                                    },
                                    { nameof(NuixAddItem.ProgressInterval), 5000 }
                                },
                                FunctionDefinition = ""
                            },
                            new ConnectionOutput
                            {
                                Result = new ConnectionOutputResult { Data = null }
                            }
                        )
                    }
                )
                .WithStepFactoryStore(UnitTestSettings);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Add file to case",
                DeleteCaseFolder,
                AssertCaseDoesNotExist,
                CreateCase,
                AssertCount(0, "*.txt"),
                new NuixAddItem
                {
                    Custodian = Constant("Mark"),
                    Paths     = DataPaths,
                    Container = Constant("New Folder")
                },
                AssertCount(2, "*.txt"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );

            yield return new NuixIntegrationTestCase(
                "Add encrypted file to case",
                DeleteCaseFolder,
                AssertCaseDoesNotExist,
                CreateCase,
                AssertCount(0, "*"),
                new NuixAddItem
                {
                    Custodian        = Constant("Mark"),
                    Paths            = EncryptedDataPaths,
                    Container        = Constant("New Folder"),
                    PasswordFilePath = (PasswordFilePath)
                },
                AssertCount(1, "princess"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );

            yield return new NuixIntegrationTestCase(
                "Add file to case with profile",
                DeleteCaseFolder,
                AssertCaseDoesNotExist,
                CreateCase,
                OpenCase,
                AssertCount(0, "*.txt"),
                new NuixAddItem
                {
                    Custodian             = Constant("Mark"),
                    Paths                 = DataPaths,
                    Container             = Constant("New Folder"),
                    ProcessingProfileName = Constant("Default")
                },
                AssertCount(2, "*.txt"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );

            yield return new NuixIntegrationTestCase(
                "Add file to case with profile path",
                DeleteCaseFolder,
                AssertCaseDoesNotExist,
                CreateCase,
                OpenCase,
                AssertCount(0, "*.txt"),
                new NuixAddItem
                {
                    Custodian             = Constant("Mark"),
                    Paths                 = DataPaths,
                    Container             = Constant("New Folder"),
                    ProcessingProfilePath = DefaultProcessingProfilePath
                },
                AssertCount(2, "*.txt"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );

            yield return new NuixIntegrationTestCase(
                "Add file to case with processing settings entity",
                DeleteCaseFolder,
                AssertCaseDoesNotExist,
                CreateCase,
                OpenCase,
                AssertCount(0, "*.txt"),
                new NuixAddItem
                {
                    Custodian          = Constant("Mark"),
                    Paths              = DataPaths,
                    Container          = Constant("New Folder"),
                    ProcessingSettings = Constant(Entity.Create(("processText", true))),
                    ProgressInterval   = Constant(100)
                },
                AssertCount(2, "*.txt"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );

            yield return new NuixIntegrationTestCase(
                "Add file to case with parallel processing settings entity",
                DeleteCaseFolder,
                AssertCaseDoesNotExist,
                CreateCase,
                OpenCase,
                AssertCount(0, "*.txt"),
                new NuixAddItem
                {
                    Custodian                  = Constant("Mark"),
                    Paths                      = DataPaths,
                    Container                  = Constant("New Folder"),
                    ParallelProcessingSettings = Constant(Entity.Create(("workerCount", 1)))
                },
                AssertCount(2, "*.txt"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );

            yield return new NuixIntegrationTestCase(
                "Add file to case with mime type settings",
                DeleteCaseFolder,
                AssertCaseDoesNotExist,
                CreateCase,
                OpenCase,
                AssertCount(0, "*.txt"),
                new NuixAddItem
                {
                    Custodian = Constant("Mark"),
                    Paths     = DataPaths,
                    Container = Constant("New Folder"),
                    MimeTypeSettings = Array(
                        Entity.Create(
                            ("mimeType", "text/plain"),
                            ("enabled", false)
                        ),
                        Entity.Create(("mimeType", "application/pdf"), ("enabled", true))
                    ),
                    CustomMetadata = Constant(Entity.Create(("Origin", "File")))
                },
                AssertCount(2, "*.txt"),
                AssertCount(0, "amethyst"),
                AssertCount(4, "evidence-metadata:\"Origin: File\""),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}
