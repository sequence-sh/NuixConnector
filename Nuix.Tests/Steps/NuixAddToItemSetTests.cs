using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public class NuixAddToItemSetTests : NuixStepTestBase<NuixAddToItemSet, Unit>
{
    /// <inheritdoc />
    public NuixAddToItemSetTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

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
                "Add To Item Set",
                DeleteCaseFolder,
                CreateCase,
                AddData,
                new NuixAddToItemSet
                {
                    SearchTerm = Constant("charm"), ItemSetName = Constant("charmset")
                },
                AssertCount(1, "item-set:charmset"),
                DeleteCaseFolder
            );
        }
    }
}

}
