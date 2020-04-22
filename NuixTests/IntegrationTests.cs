using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Connectors.Nuix.enums;
using Reductech.EDR.Connectors.Nuix.processes;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public class IntegrationTests
    {
        private static readonly List<NuixFeature> AllNuixFeatures = Enum.GetValues(typeof(NuixFeature)).Cast<NuixFeature>().ToList();

        private static readonly IReadOnlyCollection<INuixProcessSettings> NuixSettingsList = new List<INuixProcessSettings>()
        {
            new NuixProcessSettings(true, @"C:\Program Files\Nuix\Nuix 8.2\nuix_console.exe", new Version(8,2), AllNuixFeatures),
            new NuixProcessSettings(true, @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe", new Version(7,8), AllNuixFeatures),
            new NuixProcessSettings(true, @"C:\Program Files\Nuix\Nuix 7.2\nuix_console.exe", new Version(7,2), AllNuixFeatures),
            new NuixProcessSettings(true, @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe", new Version(6,2), AllNuixFeatures),
        };
        //TODO set paths from a config file, or something

        private const string Integration = "Integration";

        private static readonly string GeneralDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "IntegrationTest");

        private static readonly string CasePath = Path.Combine(GeneralDataFolder, "TestCase");
        private static readonly string OutputFolder = Path.Combine(GeneralDataFolder, "OutputFolder");
        private static readonly string ConcordanceFolder = Path.Combine(GeneralDataFolder, "ConcordanceFolder");
        private static readonly string NRTFolder = Path.Combine(GeneralDataFolder, "NRT");
        private static readonly string MigrationTestCaseFolder = Path.Combine(GeneralDataFolder, "MigrationTest");

        private static readonly string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "AllData", "data");

        private static readonly string EncryptedDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AllData", "EncryptedData");

        private static readonly string PasswordFilePath = Path.Combine(Directory.GetCurrentDirectory(), "AllData", "Passwords.txt");

        private static readonly string DefaultOCRProfilePath = Path.Combine(Directory.GetCurrentDirectory(), "DefaultOCRProfile.xml");
        private static readonly string DefaultProcessingProfilePath = Path.Combine(Directory.GetCurrentDirectory(), "DefaultProcessingProfile.xml");

        private static readonly string PoemTextImagePath = Path.Combine(Directory.GetCurrentDirectory(), "AllData", "PoemText.png");
        private static readonly string ConcordancePath = Path.Combine(Directory.GetCurrentDirectory(), "AllData", "Concordance", "loadfile.dat");
        private static readonly string MigrationPath = Path.Combine(Directory.GetCurrentDirectory(), "AllData", "MigrationTest.zip");

        private static readonly Process DeleteCaseFolder = new DeleteItem { Path = CasePath };
        private static readonly Process DeleteOutputFolder = new DeleteItem { Path = OutputFolder };
        private static readonly Process CreateOutputFolder = new CreateDirectory { Path = OutputFolder };
        private static readonly Process AssertCaseDoesNotExist = new AssertFalse { ResultOf = new NuixDoesCaseExists { CasePath = CasePath } };
        private static readonly Process CreateCase = new NuixCreateCase { CaseName = "Integration Test Case", CasePath = CasePath, Investigator = "Mark" };

        private static Process AssertFileContains(string filePath, string expectedContents)
        {
            return new AssertTrue
            {
                ResultOf = new DoesFileContain
                {
                    ExpectedContents = expectedContents,
                    FilePath = filePath
                }
            };
        }

        private static Process AssertCount(int expected, string searchTerm) =>
            new AssertTrue
            {
                ResultOf = new CheckNumber { Check = new NuixCountItems { CasePath = CasePath, SearchTerm = searchTerm }, Maximum = expected, Minimum = expected }
            };

        private static readonly Process AddData = new NuixAddItem { CasePath = CasePath, Custodian = "Mark", Path = DataPath, FolderName = "New Folder" };

        private static readonly IReadOnlyCollection<Process> TestProcesses =
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
                        ResultOf = new NuixDoesCaseExists
                        {
                            CasePath = CasePath
                        }
                    },
                    DeleteCaseFolder),

                new TestSequence("Migrate Case",
                    new DeleteItem {Path = MigrationTestCaseFolder},
                    new Unzip {ArchiveFilePath = MigrationPath, DestinationDirectory = GeneralDataFolder},
                    new AssertError {Process = new NuixSearchAndTag { CasePath = MigrationTestCaseFolder, SearchTerm = "*", Tag = "item"} }, //This should fail because we can't open the case
                    new NuixMigrateCase { CasePath = MigrationTestCaseFolder},
                    new AssertTrue{ResultOf = new CheckNumber{Maximum = 0, Minimum = 0, Check = new NuixCountItems { CasePath = MigrationTestCaseFolder, SearchTerm = "*"}}},
                    new DeleteItem {Path = MigrationTestCaseFolder}
                    ),

                new TestSequence("Add file to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = DataPath, FolderName = "New Folder"},
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder
                ),
                new TestSequence("Add encrypted file to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*"),
                    new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = EncryptedDataPath, FolderName = "New Folder", PasswordFilePath = PasswordFilePath },                    
                    AssertCount(1,"princess"),
                    DeleteCaseFolder
                ),

                new TestSequence("Add file to case with profile",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = DataPath, FolderName = "New Folder", ProcessingProfileName = "Default"},
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder
                ),

                new TestSequence("Add file to case with profile path",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = DataPath, FolderName = "New Folder", ProcessingProfilePath = DefaultProcessingProfilePath},
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
                        If = new CheckNumber{Check = new NuixCountItems {CasePath = CasePath,  SearchTerm = "*.txt"}, Maximum = 0},
                        Then = AddData
                    },
                    AssertCount(2, "*.txt"),
                    new Conditional
                    {
                        If = new CheckNumber{Check = new NuixCountItems {CasePath = CasePath,  SearchTerm = "*.txt"}, Maximum = 0},
                        Then = new AssertError(){Process= AddData },
                        Else = AssertCount(0, "*.txt")
                    },
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder
                ),

                new TestSequence("Conditionally Add file to case with nested if",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new Conditional
                    {
                        If = new CheckNumber
                        {
                            Check = new Conditional
                            {
                                If = new CheckNumber{Check = new NuixCountItems {CasePath = CasePath,  SearchTerm = "*.txt"}, Maximum = 0},
                                Then = new NuixCountItems {CasePath = CasePath,  SearchTerm = "*.txt"},
                                Else = new NuixCountItems {CasePath = CasePath,  SearchTerm = "*.txt"}
                            }, Maximum = 0
                        },

                        Then = AddData
                    },
                    AssertCount(2, "*.txt"),
                    new Conditional
                    {
                        If = new CheckNumber
                        {
                            Check = new Conditional
                            {
                                If = new CheckNumber{Check = new NuixCountItems {CasePath = CasePath,  SearchTerm = "*.txt"}, Maximum = 0},
                                Then = new NuixCountItems {CasePath = CasePath,  SearchTerm = "*.txt"},
                                Else = new NuixCountItems {CasePath = CasePath,  SearchTerm = "*.txt"}
                            }, Maximum = 0
                        },
                        Then = new AssertError(){Process= AddData },
                        Else = AssertCount(0, "*.txt")
                    },
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder
                ),


                new TestSequence("Add concordance to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddConcordance{
                        ConcordanceProfileName = "IntegrationTestProfile",
                        ConcordanceDateFormat = "yyyy-MM-dd'T'HH:mm:ss.SSSZ",
                        FilePath = ConcordancePath,
                        Custodian = "Mark",
                        FolderName = "New Folder",
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
                        CasePath = CasePath,SearchTerm = "charm",
                        Tag = "charm"
                    },
                    AssertCount(1, "tag:charm"),
                    DeleteCaseFolder

                    ),
                new TestSequence("Perform OCR",
                    DeleteCaseFolder,
                    CreateCase,
                    AssertCount(0, "sheep"),
                    new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = PoemTextImagePath, FolderName = "New Folder"},
                    new NuixPerformOCR {CasePath= CasePath, SearchTerm = "*.png"  },
                    AssertCount(1, "sheep"),
                    DeleteCaseFolder
                    ),
                new TestSequence("Perform OCR with named profile",
                    DeleteCaseFolder,
                    CreateCase,
                    AssertCount(0, "sheep"),
                    new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = PoemTextImagePath, FolderName = "New Folder"},
                    new NuixPerformOCR {CasePath= CasePath, SearchTerm = "*.png", OCRProfileName  = "Default"},
                    AssertCount(1, "sheep"),
                    DeleteCaseFolder
                ),
                new TestSequence("Perform OCR with path to profile",
                    DeleteCaseFolder,
                    CreateCase,
                    AssertCount(0, "sheep"),
                    new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = PoemTextImagePath, FolderName = "New Folder"},
                    new NuixPerformOCR {CasePath= CasePath, SearchTerm = "*.png", OCRProfilePath = DefaultOCRProfilePath},
                    AssertCount(1, "sheep"),
                    DeleteCaseFolder
                ),
                new TestSequence("Add To Item Set",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToItemSet
                    {
                        CasePath = CasePath,SearchTerm = "charm",
                        ItemSetName = "charmset"
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
                        SearchTerm = "charm",
                        ProductionSetName = "charmset",
                        ProductionProfileName = "Default"
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
                        SearchTerm = "*.txt",
                        ProductionSetName = "prodSet",
                        ProductionProfileName = "Default"
                    },
                    AssertCount(2, "production-set:prodSet"),
                    new NuixAssertPrintPreviewState
                    {
                        CasePath = CasePath,
                        ProductionSetName = "prodSet",
                        ExpectedState = PrintPreviewState.None
                    },
                    new NuixGeneratePrintPreviews
                    {
                        CasePath = CasePath,
                        ProductionSetName = "prodSet"
                    },
                    new NuixAssertPrintPreviewState
                    {
                        CasePath = CasePath,
                        ProductionSetName = "prodSet",
                        ExpectedState = PrintPreviewState.All
                    },

                    DeleteCaseFolder),

                new TestSequence("Export NRT Report",
                    DeleteCaseFolder,
                    new DeleteItem {Path = NRTFolder },
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = "*.txt",
                        ProductionSetName = "prodset",
                        ProductionProfileName = "Default"
                    },
                    new NuixCreateNRTReport
                    {
                        CasePath = CasePath,
                        NRTPath = @"C:\Program Files\Nuix\Nuix 8.2\user-data\Reports\Case Summary.nrt",
                        OutputFormat = "PDF",
                        LocalResourcesURL = @"C:\Program Files\Nuix\Nuix 8.2\user-data\Reports\Case Summary\Resources\",
                        OutputPath = NRTFolder
                    },
                    AssertFileContains(NRTFolder, "PDF-1.4"),
                    new DeleteItem {Path = NRTFolder },
                    DeleteCaseFolder

                    ),

                new TestSequence("Export Concordance",
                    DeleteCaseFolder,
                    new  DeleteItem {Path = ConcordanceFolder },
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = "charm",
                        ProductionSetName = "charmset",
                        ProductionProfileName = "Default"
                    },
                    new NuixExportConcordance
                    {
                        CasePath = CasePath,
                        ProductionSetName = "charmset",
                        ExportPath = ConcordanceFolder
                    },
                    AssertFileContains(ConcordanceFolder + "/loadfile.dat", "þDOCIDþþPARENT_DOCIDþþATTACH_DOCIDþþBEGINBATESþþENDBATESþþBEGINGROUPþþENDGROUPþþPAGECOUNTþþITEMPATHþþTEXTPATHþ"),

                    new  DeleteItem {Path = ConcordanceFolder },
                    DeleteCaseFolder
                    ),

                new TestSequence("Remove From Production Set",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = "*.txt",
                        ProductionSetName = "fullset",
                        ProductionProfileName = "Default"
                    },
                    new NuixRemoveFromProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = "Charm",
                        ProductionSetName = "fullset"
                    },
                    AssertCount(1, "production-set:fullset"),
                    DeleteCaseFolder),
                new TestSequence("Create Report",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new WriteFile()
                    {
                        Text = new NuixCreateReport
                        {
                            CasePath = CasePath,
                        },
                        Folder = OutputFolder,
                        FileName = "Stats.txt"
                    }
                    ,
                    AssertFileContains(Path.Combine(OutputFolder, "Stats.txt"),"Mark	type	text/plain	2"),

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
                        Folder = OutputFolder,
                        FileName = "Terms.txt"
                    }
                    ,
                    AssertFileContains(Path.Combine(OutputFolder, "Terms.txt"),"yellow	2"),

                    DeleteCaseFolder,
                    DeleteOutputFolder
                    ),

                new TestSequence("Extract Entities",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    //Note - we have to add items with a special profile in order to extract entities
                    new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = DataPath, FolderName = "New Folder", ProcessingProfileName = "ExtractEntities"},
                    new NuixExtractEntities
                    {
                        CasePath = CasePath,
                        OutputFolder = OutputFolder
                    },
                    AssertFileContains(OutputFolder + "/email.txt","Marianne.Moore@yahoo.com"),

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),

                new TestSequence("Irregular Items",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new WriteFile()
                    {
                        Text = new NuixCreateIrregularItemsReport
                        {
                            CasePath = CasePath
                        },
                        Folder = OutputFolder,
                        FileName = "Irregular.txt"
                    },
                    AssertFileContains(Path.Combine(OutputFolder, "Irregular.txt"),"Unrecognised\tNew Folder/data/Theme in Yellow.txt"),
                    AssertFileContains(Path.Combine(OutputFolder, "Irregular.txt"),"NeedManualExamination\tNew Folder/data/Jellyfish.txt"),
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
                        FileName = "ItemProperties.txt",
                        Folder = OutputFolder,
                        Text = new NuixGetItemProperties
                        {
                            CasePath = CasePath,
                            PropertyRegex = "(.+)",
                            SearchTerm = "*"
                        }
                    },
                    AssertFileContains(OutputFolder + "/ItemProperties.txt", "Character Set	UTF-8	New Folder/data/Jellyfish.txt"),

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),

                new TestSequence("Assign Custodians",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    AssertCount(0, "custodian:\"Jason\"" ),
                    new NuixAssignCustodian(){CasePath = CasePath, Custodian = "Jason", SearchTerm = "*"},
                    AssertCount(2, "custodian:\"Jason\"" ),
                    DeleteCaseFolder)

            };


        public static readonly IReadOnlyCollection<ProcessSettingsCombo> ProcessSettingsCombos =
            TestProcesses.SelectMany(p => NuixSettingsList.Select(s => new ProcessSettingsCombo(p, s))).Where(x => x.IsProcessCompatible).ToList();

        /// <summary>
        /// Tests just the freezing of the processes. Suitable as a unit test.
        /// </summary>
        /// <returns></returns>
        [Test]
        [TestCaseSource(nameof(ProcessSettingsCombos))]
        [TestCaseSource(nameof(ProcessSettingsCombos))]
        public void TestFreeze(ProcessSettingsCombo processSettingsCombo)
        {
            var (isSuccess, _, _, error) = processSettingsCombo.Process.TryFreeze(processSettingsCombo.Setttings);
            Assert.IsTrue(isSuccess, error?.ToString());
        }

        /// <summary>
        /// Tests freezing and execution - much slower
        /// </summary>
        /// <returns></returns>
        [Test]
        [TestCaseSource(nameof(ProcessSettingsCombos))]
        [Category(Integration)]
        public async Task TestExecution(ProcessSettingsCombo processSettingsCombo)
        {
            var (isSuccess, _, value, error) = processSettingsCombo.Process.TryFreeze(processSettingsCombo.Setttings);
            Assert.IsTrue(isSuccess, error?.ToString());

            await AssertNoErrors(value.ExecuteUntyped());
        }

        [Test]
        [Category(Integration)]
        public async Task TestVersionCheckingWithinScript()
        {
            var baseSettings = NuixSettingsList.OrderByDescending(x => x.NuixVersion).FirstOrDefault();

            Assert.IsNotNull(baseSettings);

            var process = new DoNothingRubyScriptProcess
            {
                MyRequiredVersion = new Version(100, 0)
            };

            var (freezeSuccess, _, freezeValue, freezeError) = process.TryFreeze(new NuixProcessSettings(baseSettings.UseDongle, baseSettings.NuixExeConsolePath, new Version(100, 0), baseSettings.NuixFeatures));

            Assert.IsTrue(freezeSuccess, freezeError?.ToString());

            await AssertError(freezeValue.ExecuteUntyped(), "Nuix Version is");
        }


        public static async Task<IReadOnlyCollection<string>> AssertNoErrors(IAsyncEnumerable<IProcessOutput> output)
        {
            var errors = new List<string>();
            var results = new List<string>();

            await foreach (var o in output)
            {
                if (o.OutputType == OutputType.Error)
                    errors.Add(o.Text);
                else
                {
                    results.Add(o.Text);
                }
            }

            CollectionAssert.IsEmpty(errors, string.Join("; ", results));

            return results;
        }

        public static async Task AssertError(IAsyncEnumerable<IProcessOutput> output, string expectedErrorContents)
        {
            // ReSharper disable once CollectionNeverQueried.Local - this is nice for debugging
            var results = new List<string>();

            await foreach (var o in output)
            {
                if (o.OutputType == OutputType.Error)
                {
                    if (o.Text.Contains(expectedErrorContents))
                        return;
                    else
                        Assert.Fail(o.Text);
                }
                else
                {
                    results.Add(o.Text);
                }
            }

            Assert.Fail("Expected to fail but did not.");
        }

        internal class TestSequence : Sequence
        {
            public TestSequence(string name, params Process[] steps)
            {
                Name = name;
                Steps = steps.ToList();
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return Name;
            }

            public string Name { get; }
        }


        internal class DoNothingRubyScriptProcess : RubyScriptProcess
        {
            /// <inheritdoc />
            public override string GetName() => "Do Nothing";

            /// <inheritdoc />
            internal override string ScriptText => @"
puts 'Doing Nothing'
";

            /// <inheritdoc />
            internal override string MethodName => "DoNothing";

            /// <inheritdoc />
            internal override Version RequiredVersion => MyRequiredVersion ?? new Version(1, 0);

            public Version? MyRequiredVersion { get; set; }

            /// <inheritdoc />
            internal override IReadOnlyCollection<NuixFeature> RequiredFeatures => MyRequiredFeatures ?? new List<NuixFeature>();

            public List<NuixFeature>? MyRequiredFeatures { get; set; }

            /// <inheritdoc />
            protected override NuixReturnType ReturnType => NuixReturnType.Unit;

            /// <inheritdoc />
            internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
            {
                yield break;
            }
        }
    }

    public class ProcessSettingsCombo
    {
        public ProcessSettingsCombo(Process process, INuixProcessSettings setttings)
        {
            Process = process;
            Setttings = setttings;
        }

        public readonly Process Process;

        public readonly INuixProcessSettings Setttings;

        public override string ToString()
        {
            return (Setttings.NuixVersion.ToString(2), Process.ToString()).ToString();
        }

        public bool IsProcessCompatible => IsVersionCompatible(Process, Setttings.NuixVersion);


        private static readonly Regex VersionRegex = new Regex(@"Requires Nuix Version (?<version>\d+\.\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static bool IsVersionCompatible(Process process, Version nuixVersion)
        {
            var requiredVersions = process.GetRequirements().Select(GetVersion).Where(x => x != null).ToList();

            if (process.ToString() == "Migrate Case")
                requiredVersions.Add(new Version(8, 2)); //This is a special case because the file we are trying to migrate is from 7.8



            var r = requiredVersions.All(v => nuixVersion.CompareTo(v) != -1);
            return r;
            static Version? GetVersion(string s)
            {
                var match = VersionRegex.Match(s);
                if (match.Success)
                    return Version.Parse(match.Groups["version"].Value);
                return null;
            }
        }

    }
}
