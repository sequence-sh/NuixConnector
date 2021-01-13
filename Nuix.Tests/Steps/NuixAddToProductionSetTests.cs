using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public class NuixAddToProductionSetTests : NuixStepTestBase<NuixAddToProductionSet, Unit>
{
    /// <inheritdoc />
    public NuixAddToProductionSetTests(ITestOutputHelper testOutputHelper) :
        base(testOutputHelper) { }

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
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
