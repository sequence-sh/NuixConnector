using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixSearchAndExcludeTests : NuixStepTestBase<NuixSearchAndExclude, Unit>
{
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
                new NuixSearchAndExclude
                {
                    SearchTerm = Constant("charm"),
                    Tag        = Constant("exclude|charm"),
                    SortSearch = Constant(false)
                },
                AssertCount(1, "exclusion:charm"),
                AssertCount(1, "tag:\"exclude|charm\""),
                new NuixSearchAndExclude
                {
                    SearchTerm      = Constant("yellow"),
                    ExclusionReason = Constant("color"),
                    SortSearch      = Constant(true),
                    SearchOptions   = Constant(Entity.Create(("defaultFields", new[] { "name" })))
                },
                AssertCount(1, "exclusion:color"),
                AssertCount(0, "tag:yellow"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
