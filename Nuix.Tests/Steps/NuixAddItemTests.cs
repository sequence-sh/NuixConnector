using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixAddItemTests : NuixStepTestBase<NuixAddItem, Unit>
    {
        /// <inheritdoc />
        public NuixAddItemTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}


        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new UnitTest("Add item",
                    new Sequence
                    {
                        Steps = new List<IStep<Unit>>
                        {
                            new NuixAddItem
                            {
                                CasePath = CasePath,
                                Custodian = Constant("Mark"),
                                Path = DataPath,
                                FolderName = Constant("New Folder"),
                                ProcessingSettings = Constant( CreateEntity(("Foo", "Bar")))
                            }
                        }
                    }, new List<string>(),new List<(string, string)>()
                ).WithSettings(UnitTestSettings);


            }
        }



        /// <inheritdoc />
        protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases {
            get
            {
                yield return new NuixIntegrationTestCase("Add file to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddItem
                    {
                        CasePath = (CasePath),
                        Custodian = Constant("Mark"),
                        Path = (DataPath),
                        FolderName = Constant("New Folder")
                    },
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder);


                yield return new NuixIntegrationTestCase("Add encrypted file to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*"),
                    new NuixAddItem
                    {
                        CasePath = (CasePath),
                        Custodian = Constant("Mark"),
                        Path = (EncryptedDataPath),
                        FolderName = Constant("New Folder"),
                        PasswordFilePath = (PasswordFilePath)
                    },
                    AssertCount(1, "princess"),
                    DeleteCaseFolder
                );

                yield return new NuixIntegrationTestCase("Add file to case with profile",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddItem
                    {
                        CasePath = CasePath,
                        Custodian = Constant("Mark"),
                        Path = DataPath,
                        FolderName = Constant("New Folder"),
                        ProcessingProfileName = Constant("Default")
                    },
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder
                );

                yield return new NuixIntegrationTestCase("Add file to case with profile path",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddItem
                    {
                        CasePath = CasePath,
                        Custodian = Constant("Mark"),
                        Path = DataPath,
                        FolderName = Constant("New Folder"),
                        ProcessingProfilePath = DefaultProcessingProfilePath
                    },
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder
                );

                yield return new NuixIntegrationTestCase("Conditionally Add file to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new Conditional()
                    {
                        Condition = CompareItemsCount(0, CompareOperator.LessThanOrEqual, "*.txt", CasePath),
                        ThenStep = AddData
                    },
                    AssertCount(2, "*.txt"),
                    new Conditional
                    {
                        Condition = CompareItemsCount(0, CompareOperator.LessThanOrEqual, "*.txt", CasePath),
                        ThenStep = new AssertError {Test = AddData},
                        ElseStep = AssertCount(2, "*.txt")
                    },
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder
                );


            } }
    }
}