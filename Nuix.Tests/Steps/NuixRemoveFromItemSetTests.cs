﻿using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class
    NuixRemoveFromItemSetTests : NuixStepTestBase<NuixRemoveFromItemSet, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Remove From Production Set",
                DeleteCaseFolder,
                CreateCase,
                AddData,
                new NuixAddToItemSet
                {
                    SearchTerm = Constant("charm"), ItemSetName = Constant("charmset")
                },
                AssertCount(1, "item-set:charmset"),
                new NuixRemoveFromItemSet { ItemSetName = Constant("charmset") },
                AssertCount(0, "item-set:charmset"),
                new NuixAddToItemSet
                {
                    SearchTerm = Constant("*.txt"), ItemSetName = Constant("text")
                },
                AssertCount(2, "item-set:text"),
                new NuixRemoveFromItemSet
                {
                    ItemSetName = Constant("text"),
                    SearchTerm  = Constant("jellyfish"),
                    SearchOptions =
                        Constant(Entity.Create(("defaultFields", new[] { "name" }))),
                    RemoveDuplicates = Constant(false)
                },
                AssertCount(1, "item-set:text"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
