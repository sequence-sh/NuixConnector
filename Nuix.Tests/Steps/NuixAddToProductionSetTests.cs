using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

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
                Constants.DeleteCaseFolder,
                Constants.CreateCase,
                Constants.AddData,
                new NuixAddToProductionSet
                {
                    CasePath              = Constants.CasePath,
                    SearchTerm            = Constant("charm"),
                    ProductionSetName     = Constant("charmset"),
                    ProductionProfilePath = Constants.TestProductionProfilePath
                },
                Constants.AssertCount(1, "production-set:charmset"),
                Constants.DeleteCaseFolder
            );
        }
    }
}

}
