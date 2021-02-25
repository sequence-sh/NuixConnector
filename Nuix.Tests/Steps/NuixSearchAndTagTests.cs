using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixSearchAndTagTests : NuixStepTestBase<NuixSearchAndTag, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get { yield break; }
    }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Search and tag",
                DeleteCaseFolder,
                CreateCase,
                AddData,
                new NuixSearchAndTag { SearchTerm = Constant("charm"), Tag = Constant("charm") },
                AssertCount(1, "tag:charm"),
                new NuixSearchAndTag
                {
                    SearchTerm    = Constant("\"and\""),
                    Tag           = Constant("conjunction"),
                    SortSearch    = Constant(true),
                    SearchOptions = Constant(Entity.Create(("limit", 1)))
                },
                AssertCount(1, "tag:conjunction"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
