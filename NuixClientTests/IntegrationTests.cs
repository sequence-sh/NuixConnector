using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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

        private const string directoryPath = "D:/Test/TestCase";

        public static readonly string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "data");

        [Test]
        [Category(Integration)]
        public async Task TestCreateCase()
        {
            if(Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, true);

            var sequence = new Sequence
            {
                Steps = new List<Process>
                {
                    new NuixAssertCaseExists
                    {
                        CasePath = directoryPath,
                        ShouldExist = false
                    },

                    new NuixCreateCase
                    {
                        CaseName = "Case Name",
                        CasePath = directoryPath,
                        Investigator = "Mark",
                        Description = "Description"
                    },

                    new NuixAssertCaseExists()
                    {
                        CasePath = directoryPath,
                        ShouldExist = true
                    }
                }
            };
            await AssertNoErrors(sequence.Execute(NuixSettings));

            Directory.Delete(directoryPath, true);
        }

        [Test]
        [Category(Integration)]
        public async Task TestAddFileToCase()
        {
            if(Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, true);

            var sequence = new Sequence
            {
                Steps = new List<Process>
                {
                    new NuixCreateCase
                    {
                        CaseName = "Case Name",
                        CasePath = directoryPath,
                        Investigator = "Mark",
                        Description = "Description"
                    },
                    new NuixAssertCount
                    {
                        CasePath = directoryPath,
                        ExpectedCount = 0,
                        SearchTerm = "*"
                    },

                    new NuixAddFile
                    {
                        CasePath = directoryPath,
                        Custodian = "Mark",
                        Description = "Description",
                        FilePath = DataPath,
                        FolderName = "New Folder"
                    },
                    new NuixAssertCount
                    {
                        CasePath = directoryPath,
                        ExpectedCount = 2,
                        SearchTerm = "*"
                    },
                }
            };
            await AssertNoErrors(sequence.Execute(NuixSettings));

            Directory.Delete(directoryPath, true);
        }


        [Test]
        [Category(Integration)]
        public async Task TestSearchAndTag()
        {
            if(Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, true);

            var sequence = new Sequence
            {
                Steps = new List<Process>
                {
                    new NuixCreateCase
                    {
                        CaseName = "Case Name",
                        CasePath = directoryPath,
                        Investigator = "Mark",
                        Description = "Description"
                    },
                    new NuixAddFile
                    {
                        CasePath = directoryPath,
                        Custodian = "Mark",
                        Description = "Description",
                        FilePath = DataPath,
                        FolderName = "New Folder"
                    },
                    new NuixSearchAndTag()
                    {
                        CasePath = directoryPath,SearchTerm = "charm",
                        Tag = "charm"
                    },
                    new NuixAssertCount
                    {
                        CasePath = directoryPath,
                        ExpectedCount = 1,
                        SearchTerm = "tag:charm"
                    },
                }
            };
            await AssertNoErrors(sequence.Execute(NuixSettings));

            Directory.Delete(directoryPath, true);
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
