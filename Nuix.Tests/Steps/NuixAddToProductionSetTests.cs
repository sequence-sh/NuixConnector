using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixAddToProductionSetTests : NuixStepTestBase<NuixAddToProductionSet, Unit>
{
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new NuixStepCase(
                "Create new item set and add items",
                new Sequence<Unit>
                {
                    InitialSteps = new List<IStep<Unit>> { CreateCase },
                    FinalStep = new NuixAddToProductionSet
                    {
                        SearchTerm               = Constant("toproduce*"),
                        ProductionSetName        = Constant("production"),
                        ProductionSetDescription = Constant("description"),
                        ProductionProfileName    = Constant("profile"),
                        SortSearch               = Constant(true),
                        SearchOptions            = Constant(Core.Entity.Create(("limit", 1)))
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
                                    nameof(NuixCreateCase.CaseName), "Integration Test Case"
                                },
                                { nameof(NuixCreateCase.Investigator), "Mark" }
                            }
                        },
                        new ConnectionOutput { Result = new ConnectionOutputResult { Data = null } }
                    ),
                    new(
                        new ConnectionCommand
                        {
                            Command            = "AddToProductionSet",
                            FunctionDefinition = "",
                            Arguments = new Dictionary<string, object>
                            {
                                { nameof(NuixAddToProductionSet.SearchTerm), "toproduce*" },
                                {
                                    nameof(NuixAddToProductionSet.ProductionSetName),
                                    "production"
                                },
                                {
                                    nameof(NuixAddToProductionSet.ProductionSetDescription),
                                    "description"
                                },
                                {
                                    nameof(NuixAddToProductionSet.ProductionProfileName),
                                    "profile"
                                },
                                { nameof(NuixAddToItemSet.SortSearch), true },
                                {
                                    nameof(NuixAddToItemSet.SearchOptions),
                                    CreateEntity(("limit", 1))
                                }
                            }
                        },
                        new ConnectionOutput { Result = new ConnectionOutputResult { Data = null } }
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
            yield return new NuixIntegrationTestCase(
                "Add to production set",
                DeleteCaseFolder,
                CreateCase,
                AddData,
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("charm"),
                    ProductionSetName     = Constant("charmset"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                AssertCount(1, "production-set:charmset"),
                new NuixAddToProductionSet
                {
                    SearchTerm               = Constant("\"and\""),
                    ProductionSetName        = Constant("conjunction"),
                    ProductionSetDescription = Constant("description"),
                    ProductionProfilePath    = TestProductionProfilePath,
                    SortSearch               = Constant(true),
                    SearchOptions            = Constant(Core.Entity.Create(("limit", 1)))
                },
                AssertCount(1, "production-set:conjunction"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
