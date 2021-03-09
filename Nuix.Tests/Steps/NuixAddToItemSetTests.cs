using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixAddToItemSetTests : NuixStepTestBase<NuixAddToItemSet, Unit>
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
                    FinalStep = new NuixAddToItemSet
                    {
                        SearchTerm           = Constant("green*"),
                        ItemSetName          = Constant("color"),
                        ItemSetDeduplication = Constant(ItemSetDeduplication.MD5),
                        ItemSetDescription   = Constant("A new item set"),
                        DeduplicateBy        = Constant(DeduplicateBy.Family),
                        CustodianRanking     = Array("Custodian1", "Custodian2"),
                        SortSearch           = Constant(true),
                        SearchOptions        = Constant(Entity.Create(("limit", 1)))
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
                            Command = "Search", FunctionDefinition = "", IsHelper = true
                        },
                        new ConnectionOutput
                        {
                            Result = new ConnectionOutputResult { Data = "helper_success" }
                        }
                    ),
                    new(
                        new ConnectionCommand
                        {
                            Command            = "AddToItemSet",
                            FunctionDefinition = "",
                            Arguments = new Dictionary<string, object>
                            {
                                { nameof(NuixAddToItemSet.SearchTerm), "green*" },
                                { nameof(NuixAddToItemSet.ItemSetName), "color" },
                                {
                                    nameof(NuixAddToItemSet.ItemSetDeduplication),
                                    ItemSetDeduplication.MD5.GetDisplayName()
                                },
                                {
                                    nameof(NuixAddToItemSet.ItemSetDescription),
                                    "A new item set"
                                },
                                {
                                    nameof(NuixAddToItemSet.DeduplicateBy),
                                    DeduplicateBy.Family.GetDisplayName()
                                },
                                {
                                    nameof(NuixAddToItemSet.CustodianRanking),
                                    new List<string> { "Custodian1", "Custodian2" }
                                },
                                { nameof(NuixAddToItemSet.SortSearch), true },
                                {
                                    nameof(NuixAddToItemSet.SearchOptions),
                                    Entity.Create(("limit", 1))
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
                "Add To Item Set",
                DeleteCaseFolder,
                CreateCase,
                AddData,
                new NuixAddToItemSet
                {
                    SearchTerm = Constant("charm"), ItemSetName = Constant("charmset")
                },
                AssertCount(1, "item-set:charmset"),
                new NuixAddToItemSet
                {
                    SearchTerm    = Constant("\"and\""),
                    ItemSetName   = Constant("conjunction"),
                    SortSearch    = Constant(true),
                    SearchOptions = Constant(Entity.Create(("limit", 1)))
                },
                AssertCount(1, "item-set:conjunction"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
