using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NuixClient;
using NuixClient.processes;
using NUnit.Framework;
using Processes.process;

namespace NuixClientTests
{
    class IntegrationTests
    {
        private static readonly INuixProcessSettings NuixSettings = new NuixProcessSettings(true, @"C:\Program Files\Nuix\Nuix 8.2\nuix_console.exe");
        //TODO set these from a config file

        private const string Integration = "Integration";

        [Test]
        [Category(Integration)]
        public async Task TestCreateCase()
        {
            
            const string directoryPath = "D:/Test/TestCase";

            if(Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, true);

            var sequence = new Sequence
            {
                Steps = new List<Process>
                {
                    new NuixCheckCaseExists
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

                    new NuixCheckCaseExists
                    {
                        CasePath = directoryPath,
                        ShouldExist = true
                    }
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
