using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;

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

            var process = new NuixClient.processes.NuixCreateCase()
            {
                CaseName = "Case Name",
                CasePath = directoryPath,
                Investigator = "Mark",
                Description = "Description"
            };

            await AssertNoErrors(process.Execute());

            var lines =  process.Execute();
            


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
