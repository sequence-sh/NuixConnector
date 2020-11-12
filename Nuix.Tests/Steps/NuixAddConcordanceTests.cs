using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixAddConcordanceTests : NuixStepTestBase<NuixAddConcordance, Unit>
    {
        /// <inheritdoc />
        public NuixAddConcordanceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new UnitTest("Add Concordance Test",
                    new NuixAddConcordance
                    {
                        ConcordanceProfileName = Constant("IntegrationTestProfile"),
                        ConcordanceDateFormat = Constant("yyyy-MM-dd'T'HH:mm:ss.SSSZ"),
                        FilePath = ConcordancePath,
                        Custodian = Constant("Mark"),
                        FolderName = Constant("New Folder"),
                        CasePath = CasePath
                    },
                    Unit.Default, new List<string>(),
                    new List<(string, string)>()
                    {
                        ("pathArg1a", @"IntegrationTest\TestCase"),
                        ("folderNameArg1b", "New Folder"),
                        ("folderCustodianArg1d", "Mark"),
                        ("filePathArg1e", @"Concordance\loadfile.dat"),
                        ("dateFormatArg1f", "yyyy-MM-dd'T'HH:mm:ss.SSSZ"),
                        ("profileNameArg1g", "IntegrationTestProfile")
                    }
                    )
                    .WithSettings(UnitTestSettings);

            }
        }


        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeUnitTest("Add Concordance",
                    @"NuixAddConcordance(CasePath = 'C:\Users\wainw\source\repos\Reductech\nuix\Nuix.Tests\bin\Debug\netcoreapp3.1\IntegrationTest\TestCase', ConcordanceDateFormat = 'yyyy-MM-dd''T''HH:mm:ss.SSSZ', ConcordanceProfileName = 'IntegrationTestProfile', Custodian = 'Mark', FilePath = 'C:\Users\wainw\source\repos\Reductech\nuix\Nuix.Tests\bin\Debug\netcoreapp3.1\AllData\Concordance\loadfile.dat', FolderName = 'New Folder')",
                    Unit.Default, new List<string>())
                    .WithSettings(UnitTestSettings);
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases {
            get
            {
                yield return new NuixIntegrationTestCase("Add concordance to case",
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
                );

            } }
    }
}
