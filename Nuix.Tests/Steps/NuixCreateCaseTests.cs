using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

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
                yield return new NuixStepCase("Create Case then add item",
                    new Sequence
                    {
                        Steps = new List<IStep<Unit>>
                        {
                            CreateCase,
                            AddData
                        }
                    },
                    new List<ExternalProcessAction>
                    {
                        new ExternalProcessAction(new ConnectionCommand
                        {
                            Command = "CreateCase",
                            FunctionDefinition = "",
                            Arguments = new Dictionary<string, object>
                            {
                                {nameof(NuixCreateCase.CasePath), CasePathString},
                                {nameof(NuixCreateCase.CaseName), "Integration Test Case"},
                                {nameof(NuixCreateCase.Investigator), "Mark"}
                            }
                        },
                        new ConnectionOutput
                        {
                            Result = new ConnectionOutputResult{Data = null}
                        }),
                        new ExternalProcessAction(new ConnectionCommand
                        {
                            Command = "AddToCase",
                            FunctionDefinition = "",
                            Arguments = new Dictionary<string, object>
                            {
                                {nameof(NuixAddItem.CasePath), CasePathString},
                                {nameof(NuixAddItem.FolderName), "New Folder"},
                                {nameof(NuixAddItem.Custodian), "Mark"},
                                {nameof(NuixAddItem.Path), DataPathString}
                            }
                        },
                        new ConnectionOutput
                        {
                            Result = new ConnectionOutputResult{Data = null}
                        })
                    }
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

                yield return new ErrorCase("Missing Settings", new NuixCreateCase()
                    {
                        CasePath = CasePath,
                        CaseName = Constant("Error Case"),
                        Investigator = Constant("investigator")
                    },
                    new ErrorBuilder("Could not cast 'Reductech.EDR.Core.EmptySettings' to INuixSettings", ErrorCode.MissingStepSettings)
                    );
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                var caseName = @"Integration Test Case";
                var investigator = @"Mark";
                var custodian = @"Mark";
                var folderName = @"New Folder";
                var dataPath = @"C:\Users\wainw\source\repos\Reductech\nuix\Nuix.Tests\bin\Debug\netcoreapp3.1\AllData\data";
                yield return new NuixDeserializeTest("Create Case then add item",
                    $@"- NuixCreateCase(CaseName = '{caseName}', CasePath = '{CasePathString}', Investigator = '{investigator}')
- NuixAddItem(CasePath = '{CasePathString}', Custodian = '{custodian}', FolderName = '{folderName}', Path = '{dataPath}')",
                    Unit.Default,
                    new List<ExternalProcessAction>
                    {
                        new ExternalProcessAction(new ConnectionCommand
                        {
                            Command = "CreateCase",
                            Arguments = new Dictionary<string, object>
                            {

                                {nameof(NuixCreateCase.CasePath), CasePathString},
                                {nameof(NuixCreateCase.CaseName), caseName},
                                {nameof(NuixCreateCase.Investigator), investigator}
                            }
                        }, new ConnectionOutput{Result = new ConnectionOutputResult{Data = null}}),

                        new ExternalProcessAction(new ConnectionCommand
                        {
                            Command = "AddToCase",
                            Arguments = new Dictionary<string, object>
                            {
                                {nameof(NuixAddItem.CasePath), CasePathString},
                                {nameof(NuixAddItem.Custodian),custodian},
                                {nameof(NuixAddItem.FolderName), folderName},
                                {nameof(NuixAddItem.Path), dataPath},
                            }
                        }, new ConnectionOutput{Result = new ConnectionOutputResult{Data = null}}

                        )
                    }

                    ).WithSettings(UnitTestSettings);
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
        {
            get
            {
                yield return new NuixIntegrationTestCase("Create Case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,

                    new AssertTrue
                    {
                        Test = new NuixDoesCaseExist
                        {
                            CasePath = CasePath
                        }
                    },
                    DeleteCaseFolder);


            }
        }


    }
}