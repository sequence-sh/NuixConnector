using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixCreateCaseTests : NuixStepTestBase<NuixCreateCase, Unit>
    {
        /// <inheritdoc />
        public NuixCreateCaseTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }


        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new UnitTest("Create Case then add item",
                    new Sequence
                    {
                        Steps = new List<IStep<Unit>>
                        {
                            Constants.CreateCase,
                            Constants.AddData
                        }
                    }, new List<string>(), new List<(string, string)>()
                ).WithSettings(UnitTestSettings);


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                yield return new ErrorCase("Missing Parameters", new NuixCreateCase(),
                        new ErrorBuilderList(new List<ErrorBuilder>
                        {
                            new ErrorBuilder("Missing Parameter 'CasePath' in 'CreateCase'", ErrorCode.MissingParameter),
                            new ErrorBuilder("Missing Parameter 'CaseName' in 'CreateCase'", ErrorCode.MissingParameter),
                            new ErrorBuilder("Missing Parameter 'Investigator' in 'CreateCase'", ErrorCode.MissingParameter),
                        }))
                    .WithSettings(UnitTestSettings);

                yield return new ErrorCase("Missing Settings", new NuixCreateCase(),
                    new ErrorBuilder("Could not cast 'Reductech.EDR.Core.EmptySettings' to INuixSettings", ErrorCode.MissingStepSettings)
                    );
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeUnitTest("Create Case then add item",
                    @"- NuixCreateCase(CaseName = 'Integration Test Case', CasePath = 'C:\Users\wainw\source\repos\Reductech\nuix\Nuix.Tests\bin\Debug\netcoreapp3.1\IntegrationTest\TestCase', Investigator = 'Mark')
- NuixAddItem(CasePath = 'C:\Users\wainw\source\repos\Reductech\nuix\Nuix.Tests\bin\Debug\netcoreapp3.1\IntegrationTest\TestCase', Custodian = 'Mark', FolderName = 'New Folder', Path = 'C:\Users\wainw\source\repos\Reductech\nuix\Nuix.Tests\bin\Debug\netcoreapp3.1\AllData\data')",
                    Unit.Default,
                    new List<string>()).WithSettings(UnitTestSettings);
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
        {
            get
            {
                yield return new NuixIntegrationTestCase("Create Case",
                    Constants.DeleteCaseFolder,
                    Constants.AssertCaseDoesNotExist,
                    Constants.CreateCase,

                    new AssertTrue
                    {
                        Test = new NuixDoesCaseExist
                        {
                            CasePath = Constants.CasePath
                        }
                    },
                    Constants.DeleteCaseFolder);


            }
        }


    }
}