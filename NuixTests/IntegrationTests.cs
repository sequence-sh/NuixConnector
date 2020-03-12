using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Connectors.Nuix.processes;
using Reductech.EDR.Connectors.Nuix.processes.asserts;
using Reductech.EDR.Utilities.Processes;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    class IntegrationTests
    {
        private static readonly INuixProcessSettings NuixSettings = new NuixProcessSettings(true, @"C:\Program Files\Nuix\Nuix 8.2\nuix_console.exe");
        //TODO set these from a config file

        private const string Integration = "Integration";

        private const string DirectoryPath = "D:/Test/TestCase";

        public static readonly string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "data");

        private static readonly Process DeleteCaseFolder = new DeleteItem { Path = DirectoryPath};
        private static readonly Process AssertCaseDoesNotExist = new NuixCaseExists {CasePath = DirectoryPath, ShouldExist = false};
        private static readonly Process CreateCase = new NuixCreateCase {CaseName = "Case Name", CasePath = DirectoryPath, Investigator = "Mark"};
        private static Process AssertTotalCount(int expected) => AssertCount(expected, "*");
        private static Process AssertCount(int expected, string searchTerm) => new NuixCount {CasePath = DirectoryPath, Minimum = expected, Maximum = expected,  SearchTerm = searchTerm};

        private static readonly Process AddData = new NuixAddItem {CasePath = DirectoryPath, Custodian = "Mark", Path = DataPath, FolderName = "New Folder"};

        public static readonly List<TestSequence> TestProcesses =
            new List<TestSequence>
            {
                
                new TestSequence("Create Case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    new NuixCaseExists
                    {
                        CasePath = DirectoryPath,
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
                        CasePath = DirectoryPath,SearchTerm = "charm",
                        Tag = "charm"
                    },
                    AssertCount(1, "tag:charm"),
                    DeleteCaseFolder
                    
                    ),
                //TODO AddConcordance
                new TestSequence("Add To Item Set", 
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToItemSet
                    {
                        CasePath = DirectoryPath,SearchTerm = "charm",
                        ItemSetName = "charm set"
                    },
                    AssertCount(1, "item-Set:\"charm set\""),
                    DeleteCaseFolder),
                new TestSequence("Add To Production Set", 
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet()
                    {
                        CasePath = DirectoryPath,SearchTerm = "charm",
                        ProductionSetName = "charm set"
                    },
                    AssertCount(1, "production-Set:\"charm set\""),
                    DeleteCaseFolder),
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
                Steps = steps.ToList()
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return Name;
            }

            public string Name { get; }
        }
    }
}
