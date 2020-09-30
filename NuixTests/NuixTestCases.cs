using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.General;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public static class NuixTestCases
    {
        //TODO set paths from a config file, or something

        public const string Integration = "Integration";
        public const string Category = "Category";

        private static readonly string GeneralDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "IntegrationTest");

        private static readonly IStep<string> CasePath = Constant(Path.Combine(GeneralDataFolder, "TestCase"));
        private static readonly string OutputFolder = Path.Combine(GeneralDataFolder, "OutputFolder");
        private static readonly string ConcordanceFolder = Path.Combine(GeneralDataFolder, "ConcordanceFolder");
        private static readonly IStep<string> NRTFolder = Constant(Path.Combine(GeneralDataFolder, "NRT"));
        private static readonly IStep<string> MigrationTestCaseFolder = Constant(Path.Combine(GeneralDataFolder, "MigrationTest"));

        private static readonly IStep<string> DataPath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "data"));

        private static readonly IStep<string> EncryptedDataPath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "EncryptedData"));

        private static readonly IStep<string> PasswordFilePath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "Passwords.txt"));

        //private static readonly string DefaultOCRProfilePath = Path.Combine(Directory.GetCurrentDirectory(), "AllData", "DefaultOCRProfile.xml");
        private static readonly IStep<string> DefaultProcessingProfilePath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "DefaultProcessingProfile.xml"));
        private static readonly IStep<string> TestProductionProfilePath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "IntegrationTestProductionProfile.xml"));

        private static readonly IStep<string> PoemTextImagePath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "PoemText.png"));
        private static readonly IStep<string> ConcordancePath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "Concordance", "loadfile.dat"));
        private static readonly IStep<string> MigrationPath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "MigrationTest.zip"));

        private static readonly IStep<Unit> DeleteCaseFolder = new DeleteItem { Path = CasePath };
        private static readonly IStep<Unit> DeleteOutputFolder = new DeleteItem { Path = Constant(OutputFolder) };
        private static readonly IStep<Unit> CreateOutputFolder = new CreateDirectory { Path = Constant(OutputFolder) };
        private static readonly IStep<Unit> AssertCaseDoesNotExist = new AssertTrue { Test = new Not { Boolean = new NuixDoesCaseExists { CasePath = CasePath } } };
        private static readonly IStep<Unit> CreateCase = new NuixCreateCase
        {
            CaseName = Constant("Integration Test Case"),
            CasePath = CasePath,
            Investigator = Constant("Mark")
        };

        private static IStep<Unit> AssertFileContains(string folderName, string fileName, string expectedContents)
        {
            var path = Constant(Path.Combine(folderName, fileName));

            return new AssertTrue
            {
                Test = new DoesFileContain
                {
                    Text = new Constant<string>(expectedContents),
                    Path = path
                }
            };
        }

        private static IStep<T> Constant<T>(T c) => new Constant<T>(c);

        private static IStep<Unit> AssertCount(int expected, string searchTerm, IStep<string>? casePath = null) =>
            new AssertTrue
            {
                Test = CompareItemsCount(expected, CompareOperator.Equals, searchTerm, casePath)
            };

        private static IStep<bool> CompareItemsCount(int right, CompareOperator op, string searchTerm, IStep<string>? casePath)
        {
            return new Compare<int>
            {
                Left = new Constant<int>(right),
                Operator = new Constant<CompareOperator>(op),
                Right = new NuixCountItems
                {
                    CasePath = casePath ?? CasePath,
                    SearchTerm = Constant(searchTerm)
                }
            };
        }

        private static readonly IStep<Unit> AddData = new NuixAddItem
        {
            CasePath = CasePath,
            Custodian = Constant("Mark"),
            Path = DataPath,
            FolderName = Constant("New Folder")
        };

        private static readonly IReadOnlyCollection<TestSequence> TestSequences =
            new List<TestSequence>
            {
                //TODO AnnotateDocumentIdList
                //TODO ImportDocumentIds

                new TestSequence("Create Case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,

                    new AssertTrue
                    {
                        Test = new NuixDoesCaseExists
                            {
                                CasePath = CasePath
                            }
                    },
                    DeleteCaseFolder),

                new TestSequence("Migrate Case",
                    new DeleteItem {Path = MigrationTestCaseFolder},
                    new Unzip {ArchiveFilePath = MigrationPath, DestinationDirectory = Constant(GeneralDataFolder)},
                    new AssertError
                    {
                        Test = new NuixSearchAndTag
                            {CasePath = MigrationTestCaseFolder, SearchTerm = Constant("*"), Tag = Constant("item")}
                    }, //This should fail because we can't open the case
                    new NuixMigrateCase {CasePath = MigrationTestCaseFolder},
                    AssertCount(1, "jellyfish.txt", MigrationTestCaseFolder),
                    new DeleteItem {Path = MigrationTestCaseFolder}
                ),

                new TestSequence("Add file to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddItem
                    {
                        CasePath = (CasePath), Custodian = Constant("Mark"), Path = (DataPath),
                        FolderName = Constant("New Folder")
                    },
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder
                ),
                new TestSequence("Add encrypted file to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*"),
                    new NuixAddItem
                    {
                        CasePath = (CasePath), Custodian = Constant("Mark"), Path = (EncryptedDataPath),
                        FolderName = Constant("New Folder"), PasswordFilePath = (PasswordFilePath)
                    },
                    AssertCount(1, "princess"),
                    DeleteCaseFolder
                ),

                new TestSequence("Add file to case with profile",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddItem
                    {
                        CasePath = CasePath, Custodian = Constant("Mark"), Path = DataPath,
                        FolderName = Constant("New Folder"), ProcessingProfileName = Constant("Default")
                    },
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder
                ),

                new TestSequence("Add file to case with profile path",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddItem
                    {
                        CasePath = CasePath, Custodian = Constant("Mark"), Path = DataPath,
                        FolderName = Constant("New Folder"), ProcessingProfilePath = DefaultProcessingProfilePath
                    },
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder
                ),

                new TestSequence("Conditionally Add file to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new Conditional()
                    {
                        Condition = CompareItemsCount(0, CompareOperator.LessThanOrEqual, "*.txt", CasePath),
                        ThenStep = AddData
                    },
                    AssertCount(2, "*.txt"),
                    new Conditional
                    {
                        Condition = CompareItemsCount(0, CompareOperator.LessThanOrEqual, "*.txt", CasePath),
                        ThenStep = new AssertError {Test = AddData},
                        ElseStep = AssertCount(2, "*.txt")
                    },
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder
                ),

                new TestSequence("Add concordance to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddConcordance
                    {
                        ConcordanceProfileName = Constant("IntegrationTestProfile"),
                        ConcordanceDateFormat = Constant("yyyy-MM-dd'T'HH:mm:ss.SSSZ"),
                        FilePath = ConcordancePath,
                        Custodian = Constant("Mark"),
                        FolderName = Constant("New Folder"),
                        CasePath = CasePath
                    },
                    AssertCount(1, "*.txt"),
                    DeleteCaseFolder
                ),


                new TestSequence("Search and tag",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixSearchAndTag
                    {
                        CasePath = CasePath, SearchTerm = Constant("charm"),
                        Tag = Constant("charm")
                    },
                    AssertCount(1, "tag:charm"),
                    DeleteCaseFolder

                ),
                new TestSequence("Perform OCR",
                    DeleteCaseFolder,
                    CreateCase,
                    AssertCount(0, "sheep"),
                    new NuixAddItem
                    {
                        CasePath = CasePath, Custodian = Constant("Mark"), Path = PoemTextImagePath,
                        FolderName = Constant("New Folder")
                    },
                    new NuixPerformOCR {CasePath = CasePath, SearchTerm = Constant("*.png")},
                    AssertCount(1, "sheep"),
                    DeleteCaseFolder
                ),
                new TestSequence("Perform OCR with named profile",
                    DeleteCaseFolder,
                    CreateCase,
                    AssertCount(0, "sheep"),
                    new NuixAddItem
                    {
                        CasePath = CasePath, Custodian = Constant("Mark"), Path = PoemTextImagePath,
                        FolderName = Constant("New Folder")
                    },
                    new NuixPerformOCR
                        {CasePath = CasePath, SearchTerm = Constant("*.png"), OCRProfileName = Constant("Default")},
                    AssertCount(1, "sheep"),
                    DeleteCaseFolder
                ),
                //new TestSequence("Perform OCR with path to profile",
                //    DeleteCaseFolder,
                //    CreateCase,
                //    AssertCount(0, "sheep"),
                //    new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = PoemTextImagePath, FolderName = "New Folder"},
                //    new NuixPerformOCR {CasePath= CasePath, SearchTerm = "*.png", OCRProfilePath = DefaultOCRProfilePath},
                //    AssertCount(1, "sheep"),
                //    DeleteCaseFolder
                //),
                new TestSequence("Add To Item Set",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToItemSet
                    {
                        CasePath = CasePath, SearchTerm = Constant("charm"),
                        ItemSetName = Constant("charmset")
                    },
                    AssertCount(1, "item-set:charmset"),
                    DeleteCaseFolder),

                new TestSequence("Add To Production Set",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = Constant("charm"),
                        ProductionSetName = Constant("charmset"),
                        ProductionProfilePath = TestProductionProfilePath
                    },
                    AssertCount(1, "production-set:charmset"),
                    DeleteCaseFolder),


                new TestSequence("Generate Print Previews",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = Constant("*.txt"),
                        ProductionSetName = Constant("prodSet"),
                        ProductionProfilePath = TestProductionProfilePath
                    },
                    AssertCount(2, "production-set:prodSet"),
                    new NuixAssertPrintPreviewState
                    {
                        CasePath = CasePath,
                        ProductionSetName = Constant("prodSet"),
                        ExpectedState = Constant(PrintPreviewState.None)
                    },
                    new NuixGeneratePrintPreviews
                    {
                        CasePath = CasePath,
                        ProductionSetName = Constant("prodSet")
                    },
                    new NuixAssertPrintPreviewState
                    {
                        CasePath = CasePath,
                        ProductionSetName = Constant("prodSet"),
                        ExpectedState = Constant(PrintPreviewState.All)
                    },

                    DeleteCaseFolder),

                new TestSequence("Export NRT Report",
                    DeleteCaseFolder,
                    new DeleteItem {Path = NRTFolder},
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = Constant("*.txt"),
                        ProductionSetName = Constant("prodset"),
                        ProductionProfilePath = TestProductionProfilePath
                    },
                    new NuixCreateNRTReport
                    {
                        CasePath = CasePath,
                        NRTPath = Constant(@"C:\Program Files\Nuix\Nuix 8.2\user-data\Reports\Case Summary.nrt"),
                        OutputFormat = Constant("PDF"),
                        LocalResourcesURL =
                            Constant(@"C:\Program Files\Nuix\Nuix 8.2\user-data\Reports\Case Summary\Resources\"),
                        OutputPath = NRTFolder
                    },
                    AssertFileContains(GeneralDataFolder, "NRT", "PDF-1.4"),
                    new DeleteItem {Path = NRTFolder},
                    DeleteCaseFolder

                ),

                new TestSequence("Export Concordance",
                    DeleteCaseFolder,
                    new DeleteItem {Path = Constant(ConcordanceFolder)},
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = Constant("charm"),
                        ProductionSetName = Constant("charmset"),
                        ProductionProfilePath = TestProductionProfilePath
                    },
                    new NuixExportConcordance
                    {
                        CasePath = CasePath,
                        ProductionSetName = Constant("charmset"),
                        ExportPath = Constant(ConcordanceFolder)
                    },
                    AssertFileContains(ConcordanceFolder, "loadfile.dat", "DOCID"),
                    AssertFileContains(ConcordanceFolder, "TEXT/000/000/DOC-000000001.txt", "Visible, invisible"),
                    new DeleteItem {Path = new Constant<string>(ConcordanceFolder)},
                    DeleteCaseFolder
                ),

                new TestSequence("Remove From Production Set",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = Constant("*.txt"),
                        ProductionSetName = Constant("fullset"),
                        ProductionProfilePath = TestProductionProfilePath
                    },
                    new NuixRemoveFromProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = Constant("Charm"),
                        ProductionSetName = Constant("fullset")
                    },
                    AssertCount(1, "production-set:fullset"),
                    DeleteCaseFolder),
                new TestSequence("Create Report",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new WriteFile
                    {
                        Text = new NuixCreateReport
                        {
                            CasePath = CasePath,
                        },
                        Folder = Constant(OutputFolder),
                        FileName = new Constant<string>("Stats.txt")
                    },
                    AssertFileContains(OutputFolder, "Stats.txt", "Mark	type	text/plain	2"),

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),
                new TestSequence("Create Term List",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new WriteFile
                    {
                        Text = new NuixCreateTermList
                        {
                            CasePath = CasePath,

                        },
                        Folder = Constant(OutputFolder),
                        FileName = new Constant<string>("Terms.txt")
                    }
                    ,
                    AssertFileContains(OutputFolder, "Terms.txt", "yellow	2"),

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),

                new TestSequence("Extract Entities",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    //Note - we have to add items with a special profile in order to extract entities
                    new NuixAddItem
                    {
                        CasePath = CasePath, Custodian = Constant("Mark"), Path = DataPath,
                        FolderName = Constant("New Folder"), ProcessingProfileName = Constant("ExtractEntities")
                    },
                    new NuixExtractEntities
                    {
                        CasePath = CasePath,
                        OutputFolder = Constant(OutputFolder)
                    },
                    AssertFileContains(OutputFolder, "email.txt", "Marianne.Moore@yahoo.com"),

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),

                new TestSequence("Irregular Items",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new WriteFile
                    {
                        Text = new NuixCreateIrregularItemsReport
                        {
                            CasePath = CasePath
                        },
                        Folder = Constant(OutputFolder),
                        FileName = new Constant<string>("Irregular.txt")
                    },
                    AssertFileContains(OutputFolder, "Irregular.txt",
                        "Unrecognised\tNew Folder/data/Theme in Yellow.txt"),
                    AssertFileContains(OutputFolder, "Irregular.txt",
                        "NeedManualExamination\tNew Folder/data/Jellyfish.txt"),
                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),

                new TestSequence("Get Item Properties",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new WriteFile
                    {
                        FileName = new Constant<string>("ItemProperties.txt"),
                        Folder = Constant(OutputFolder),
                        Text = new NuixGetItemProperties
                        {
                            CasePath = CasePath,
                            PropertyRegex = Constant("(.+)"),
                            SearchTerm = Constant("*")
                        }
                    },
                    AssertFileContains(OutputFolder, "ItemProperties.txt",
                        "Character Set	UTF-8	New Folder/data/Jellyfish.txt"),

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),

                new TestSequence("Assign Custodians",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    AssertCount(0, "custodian:\"Jason\""),
                    new NuixAssignCustodian()
                        {CasePath = CasePath, Custodian = Constant("Jason"), SearchTerm = Constant("*")},
                    AssertCount(4, "custodian:\"Jason\""),
                    DeleteCaseFolder)

            };

        internal class TestSequence
        {
            public TestSequence(string name, params IStep<Unit>[] steps)
            {
                Name = name;
                Sequence = new Sequence()
                {
                    Steps = steps
                };
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return Name;
            }

            public string Name { get; }

            public Sequence Sequence { get; }
        }


        public static IEnumerable<StepSettingsCombo> GetSettingsCombos()
        {
            foreach (var nuixSettings in NuixSettingsList)
            {
                foreach (var testSequence in TestSequences)
                {
                    var combo = new StepSettingsCombo(testSequence.Name, testSequence.Sequence, nuixSettings);
                    if (combo.IsStepCompatible)
                        yield return combo;
                }
            }
        }


        private static readonly List<NuixFeature> AllNuixFeatures = Enum.GetValues(typeof(NuixFeature)).Cast<NuixFeature>().ToList();

        public static readonly IReadOnlyCollection<INuixSettings> NuixSettingsList =
            new List<INuixSettings>
            {
                new NuixSettings(true, @"C:\Program Files\Nuix\Nuix 8.2\nuix_console.exe", new Version(8, 2),
                    AllNuixFeatures),
                //new NuixSettings(true, @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe", new Version(7, 8), AllNuixFeatures),
                new NuixSettings(true, @"C:\Program Files\Nuix\Nuix 7.2\nuix_console.exe", new Version(7, 2),
                    AllNuixFeatures),
                new NuixSettings(true, @"C:\Program Files\Nuix\Nuix 6.2\nuix_console.exe", new Version(6, 2),
                    AllNuixFeatures),
            };

    }
}