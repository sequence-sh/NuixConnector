using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Connectors.Nuix.processes;
using Reductech.EDR.Connectors.Nuix.processes.asserts;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    class IntegrationTests
    {
        private static readonly INuixProcessSettings NuixSettings = new NuixProcessSettings(true, @"C:\Program Files\Nuix\Nuix 8.2\nuix_console.exe");
        //TODO set these from a config file

        private const string Integration = "Integration";

        private const string CasePath = "D:/Test/TestCase";
        private const string OutputFolder = "D:/Test/OutputFolder";

        public static readonly string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "data");

        private static readonly Process DeleteCaseFolder = new DeleteItem { Path = CasePath};
        private static readonly Process DeleteOutputFolder = new DeleteItem { Path = OutputFolder};
        private static readonly Process AssertCaseDoesNotExist = new NuixCaseExists {CasePath = CasePath, ShouldExist = false};
        private static readonly Process CreateCase = new NuixCreateCase {CaseName = "Case Name", CasePath = CasePath, Investigator = "Mark"};
        private static Process AssertTotalCount(int expected) => AssertCount(expected, "*");
        private static Process AssertCount(int expected, string searchTerm) => new NuixCount {CasePath = CasePath, Minimum = expected, Maximum = expected,  SearchTerm = searchTerm};

        private static readonly Process AddData = new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = DataPath, FolderName = "New Folder"};

        public static readonly List<TestSequence> TestProcesses =
            new List<TestSequence>
            {
                //TODO AddConcordance
                //TODO AnnotateDocumentIdList
                //TODO CreateNRTReport
                //TODO ExportConcordance
                //TODO ImportDocumentIds
                //TODO MigrateCase
                //TODO perform OCR
                //TODO generate print previews

                
                new TestSequence("Create Case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    new NuixCaseExists
                    {
                        CasePath = CasePath,
                        ShouldExist = true
                    },
                    DeleteCaseFolder),
                new TestSequence("Add file to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    AssertTotalCount(0),
                    CreateCase,
                    AddData,
                    AssertTotalCount(2),
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
                
                new TestSequence("Add To Item Set", 
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToItemSet
                    {
                        CasePath = CasePath,SearchTerm = "charm",
                        ItemSetName = "charm set"
                    },
                    AssertCount(1, "item-Set:\"charm set\""),
                    DeleteCaseFolder),

                new TestSequence("Add To Production Set", 
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = "charm",
                        ProductionSetName = "charm set"
                    },
                    AssertCount(1, "production-Set:\"charm set\""),
                    DeleteCaseFolder),

                new TestSequence("Remove From Production Set", 
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = "*",
                        ProductionSetName = "full set"
                    },
                    new NuixRemoveFromProductionSet()
                    {
                        CasePath = CasePath,
                        SearchTerm = "Charm",
                        ProductionSetName = "full set"
                    },
                    AssertCount(1, "production-Set:\"full set\""),
                    DeleteCaseFolder),
                

                new TestSequence("Create Irregular Items Report",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateCase,
                    AddData,
                    new NuixCreateIrregularItemsReport
                    {
                        CasePath = CasePath,
                        OutputFolder = OutputFolder
                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/file.txt", //TODO set these field
                        ExpectedContents = "ABCD"
                    },

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),
                new TestSequence("Create Report",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateCase,
                    AddData,
                    new NuixCreateReport
                    {
                        CasePath = CasePath,
                        OutputFolder = OutputFolder
                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/file.txt", //TODO set these field
                        ExpectedContents = "ABCD"
                    },

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),
                new TestSequence("Create Term List",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateCase,
                    AddData,
                    new NuixCreateTermList
                    {
                        CasePath = CasePath,
                        OutputFolder = OutputFolder
                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/file.txt", //TODO set these field
                        ExpectedContents = "ABCD"
                    },

                    DeleteCaseFolder,
                    DeleteOutputFolder
                    ),

                new TestSequence("Extract Entities",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateCase,
                    AddData,
                    new NuixExtractEntities
                    {
                        CasePath = CasePath,
                        OutputFolder = OutputFolder
                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/file.txt", //TODO set these field
                        ExpectedContents = "ABCD"
                    },

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),

                new TestSequence("Irregular Items",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateCase,
                    AddData,
                    new NuixCreateIrregularItemsReport()
                    {
                        CasePath = CasePath,
                        OutputFolder = OutputFolder
                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/file.txt", //TODO set these field
                        ExpectedContents = "ABCD"
                    },

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),

                new TestSequence("Get Item Properties",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateCase,
                    AddData,
                    new NuixGetItemProperties()
                    {
                        CasePath = CasePath,
                        OutputFolder = OutputFolder,
                        OutputFileName = "ItemProperties",
                        PropertyRegex = ".+",
                        SearchTerm = "*"

                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/ItemProperties.txt", //TODO set these field
                        ExpectedContents = "ABCD"
                    },

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),

            };



        [Test]
        [TestCaseSource(nameof(TestProcesses))]
        [Category(Integration)]
        public async Task Test(Sequence sequence)
        {
            await AssertNoErrors(sequence.Execute(NuixSettings));
        }


        private static async Task AssertNoErrors(IAsyncEnumerable<Result<string>> lines)
        {
            var errors = new List<string>();

            await foreach (var (_, isFailure, _, error) in lines)
            {
                if (isFailure)
                    errors.Add(error);
            }
            
            CollectionAssert.IsEmpty(errors);
        }

        public class TestSequence : Sequence
        {
            public TestSequence(string name, params  Process[] steps)
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

        public class AssertFileContents : Process
        {
            [DataMember]
            [Required]
            [YamlMember]
            public string FilePath { get; set; }

            [DataMember]
            [Required]
            [YamlMember]
            public string ExpectedContents { get;set; }

            /// <inheritdoc />
            public override IEnumerable<string> GetArgumentErrors()
            {
                if (string.IsNullOrWhiteSpace(FilePath))
                    yield return "FilePath is empty";
            }

            /// <inheritdoc />
            public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
            {
                yield break;
            }

            /// <inheritdoc />
            public override string GetName()
            {
                return "Assert file contains";
            }

            /// <inheritdoc />
            public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
            {
                if (!File.Exists(FilePath))
                    yield return Result.Failure<string>("File does not exist");
                else
                {
                    var text = await File.ReadAllTextAsync(FilePath);

                    if (ExpectedContents == text)
                        yield return Result.Success("Contents Match");
                    else
                    {
                        yield return Result.Failure<string>("Contents do not match");
                    }
                }
                
            }
        }
    }
}
