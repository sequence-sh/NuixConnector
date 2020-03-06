using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Processes.process;

namespace NuixClientTests
{
    class IntegrationTests
    {
        private const string Integration = "Integration";

        [Test]
        [Category(Integration)]
        public async Task TestCreateCase()
        {
            const string directoryPath = "D:/Test/TestCase";

            Directory.Delete(directoryPath, true);

            var sequence = new Sequence()
            {
                Steps = new List<Process>()
                {
                    new NuixClient.processes.NuixCheckCaseExists()
                    {
                        CasePath = directoryPath,
                        ShouldExist = false
                    },

                    new NuixClient.processes.NuixCreateCase()
                    {
                        CaseName = "Case Name",
                        CasePath = directoryPath,
                        Investigator = "Mark",
                        Description = "Description"
                    },

                    new NuixClient.processes.NuixCheckCaseExists()
                    {
                        CasePath = directoryPath,
                        ShouldExist = true
                    },
                }
            };
            await AssertNoErrors(sequence.Execute());

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
