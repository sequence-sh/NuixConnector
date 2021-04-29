using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixCreateCaseTests : NuixStepTestBase<NuixCreateCase, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new NuixStepCase(
                    "Create Case then add item",
                    new Sequence<Unit>
                    {
                        InitialSteps = new List<IStep<Unit>> { CreateCase },
                        FinalStep = new NuixAddItem
                        {
                            Custodian = Constant("Mark"),
                            Paths     = DataPaths,
                            Container = Constant("New Folder")
                        }
                    },
                    new List<ExternalProcessAction>
                    {
                        new(
                            new ConnectionCommand
                            {
                                Command            = "CreateCase",
                                FunctionDefinition = "",
                                Arguments = new Dictionary<string, object>
                                {
                                    { nameof(NuixCreateCase.CasePath), CasePathString },
                                    {
                                        nameof(NuixCreateCase.CaseName),
                                        "Integration Test Case"
                                    },
                                    { nameof(NuixCreateCase.Investigator), "Mark" }
                                }
                            },
                            new ConnectionOutput
                            {
                                Result = new ConnectionOutputResult { Data = null }
                            }
                        ),
                        new(
                            new ConnectionCommand
                            {
                                Command            = "AddToCase",
                                FunctionDefinition = "",
                                Arguments = new Dictionary<string, object>
                                {
                                    { nameof(NuixAddItem.Container), "New Folder" },
                                    { nameof(NuixAddItem.Custodian), "Mark" },
                                    {
                                        nameof(NuixAddItem.Paths),
                                        new List<string> { DataPathString }
                                    },
                                    { nameof(NuixAddItem.ProgressInterval), 5000 }
                                }
                            },
                            new ConnectionOutput
                            {
                                Result = new ConnectionOutputResult { Data = null }
                            }
                        )
                    }
                )
                .WithSettings(UnitTestSettings);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                    "Missing Parameters",
                    new NuixCreateCase(),
                    new ErrorBuilderList(
                        new List<ErrorBuilder>
                        {
                            new(ErrorCode.MissingParameter, "CasePath"),
                            new(ErrorCode.MissingParameter, "CaseName"),
                            new(ErrorCode.MissingParameter, "Investigator"),
                        }
                    )
                )
                .WithSettings(UnitTestSettings);

            yield return new ErrorCase(
                "Missing Settings",
                new NuixCreateCase
                {
                    CasePath     = CasePath,
                    CaseName     = Constant("Error Case"),
                    Investigator = Constant("investigator")
                },
                new ErrorBuilder(ErrorCode.MissingStepSettingsValue, "Settings", "ExeConsolePath")
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            var caseName     = @"Integration Test Case";
            var investigator = @"Mark";
            var custodian    = @"Mark";
            var folderName   = @"New Folder";

            var dataPath =
                @"C:\Users\wainw\source\repos\Reductech\nuix\Nuix.Tests\bin\Debug\netcoreapp3.1\AllData\data";

            yield return new NuixDeserializeTest(
                    "Create Case then add item",
                    $@"- NuixCreateCase CaseName: '{caseName}' CasePath: '{CasePathString}' Investigator: '{investigator}'
- NuixAddItem Custodian: '{custodian}' Container: '{folderName}' Paths: ['{dataPath}']",
                    Unit.Default,
                    new List<ExternalProcessAction>
                    {
                        new(
                            new ConnectionCommand
                            {
                                Command = "CreateCase",
                                Arguments = new Dictionary<string, object>
                                {
                                    { nameof(NuixCreateCase.CasePath), CasePathString },
                                    { nameof(NuixCreateCase.CaseName), caseName },
                                    { nameof(NuixCreateCase.Investigator), investigator }
                                }
                            },
                            new ConnectionOutput
                            {
                                Result = new ConnectionOutputResult { Data = null }
                            }
                        ),
                        new(
                            new ConnectionCommand
                            {
                                Command = "AddToCase",
                                Arguments = new Dictionary<string, object>
                                {
                                    { nameof(NuixAddItem.Custodian), custodian },
                                    { nameof(NuixAddItem.Container), folderName },
                                    {
                                        nameof(NuixAddItem.Paths),
                                        new List<string> { dataPath }
                                    },
                                    { nameof(NuixAddItem.ProgressInterval), 5000 }
                                }
                            },
                            new ConnectionOutput
                            {
                                Result = new ConnectionOutputResult { Data = null }
                            }
                        )
                    }
                ).WithScriptExists()
                .WithSettings(UnitTestSettings);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Create Case",
                DeleteCaseFolder,
                AssertCaseDoesNotExist,
                CreateCase,
                new AssertTrue { Boolean = new NuixDoesCaseExist { CasePath = CasePath } },
                CleanupCase
            );
        }
    }
}

}
