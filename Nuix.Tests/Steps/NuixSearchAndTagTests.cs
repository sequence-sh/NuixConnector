using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public class NuixSearchAndTagTests : NuixStepTestBase<NuixSearchAndTag, Unit>
{
    /// <inheritdoc />
    public NuixSearchAndTagTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

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
                DeleteCaseFolder
            );
        }
    }
}

}
