using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NuixClient;
using NuixClient.processes;
using NuixClient.processes.asserts;
using NUnit.Framework;
using Processes.process;

namespace NuixClientTests
{
    class IntegrationTests
    {
        private static readonly INuixProcessSettings NuixSettings = new NuixProcessSettings(true, @"C:\Program Files\Nuix\Nuix 8.2\nuix_console.exe");
        //TODO set these from a config file

        private const string Integration = "Integration";

        private const string DirectoryPath = "D:/Test/TestCase";

        public static readonly string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "data");

        [Test]
        [Category(Integration)]
        public async Task TestCreateCase()
        {
            var sequence = new Sequence
            {
                Steps = new List<Process>
                {
                    new DeleteFile
                    {
                        Path = DirectoryPath
                    },
                    new NuixAssertCaseExists
                    {
                        CasePath = DirectoryPath,
                        ShouldExist = false
                    },

                    new NuixCreateCase
                    {
                        CaseName = "Case Name",
                        CasePath = DirectoryPath,
                        Investigator = "Mark",
                        Description = "Description"
                    },

                    new NuixAssertCaseExists
                    {
                        CasePath = DirectoryPath,
                        ShouldExist = true
                    },

                    new NuixAssertCaseExists
                    {
                        CasePath = DirectoryPath,
                        ShouldExist = false
                    },
                    new DeleteFile
                    {
                        Path = DirectoryPath
                    },
                }
            };
            await AssertNoErrors(sequence.Execute(NuixSettings));
        }

        [Test]
        [Category(Integration)]
        public async Task TestAddFileToCase()
        {
            var sequence = new Sequence
            {
                Steps = new List<Process>
                {
                    new DeleteFile
                    {
                        Path = DirectoryPath
                    },
                    new NuixCreateCase
                    {
                        CaseName = "Case Name",
                        CasePath = DirectoryPath,
                        Investigator = "Mark",
                        Description = "Description"
                    },
                    new NuixAssertCount
                    {
                        CasePath = DirectoryPath,
                        ExpectedCount = 0,
                        SearchTerm = "*"
                    },

                    new NuixAddFile
                    {
                        CasePath = DirectoryPath,
                        Custodian = "Mark",
                        Description = "Description",
                        FilePath = DataPath,
                        FolderName = "New Folder"
                    },
                    new NuixAssertCount
                    {
                        CasePath = DirectoryPath,
                        ExpectedCount = 2,
                        SearchTerm = "*"
                    },
                    new DeleteFile
                    {
                        Path = DirectoryPath
                    },
                }
            };
            await AssertNoErrors(sequence.Execute(NuixSettings));
        }


        [Test]
        [Category(Integration)]
        public async Task TestSearchAndTag()
        {

            var sequence = new Sequence
            {
                Steps = new List<Process>
                {
                    new DeleteFile
                    {
                        Path = DirectoryPath
                    },

                    new NuixCreateCase
                    {
                        CaseName = "Case Name",
                        CasePath = DirectoryPath,
                        Investigator = "Mark",
                        Description = "Description"
                    },
                    new NuixAddFile
                    {
                        CasePath = DirectoryPath,
                        Custodian = "Mark",
                        Description = "Description",
                        FilePath = DataPath,
                        FolderName = "New Folder"
                    },
                    new NuixSearchAndTag()
                    {
                        CasePath = DirectoryPath,SearchTerm = "charm",
                        Tag = "charm"
                    },
                    new NuixAssertCount
                    {
                        CasePath = DirectoryPath,
                        ExpectedCount = 1,
                        SearchTerm = "tag:charm"
                    },
                    new DeleteFile
                    {
                        Path = DirectoryPath
                    },
                }
            };
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
    }
}
