using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class
    NuixReorderProductionSetTests : NuixStepTestBase<NuixReorderProductionSet, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases { get { yield break; } }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Reorder From Production Set",
                SetupCase,
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("*.txt"),
                    ProductionSetName     = Constant("fullset"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                new NuixReorderProductionSet()
                {
                    ProductionSetName = Constant("fullset"),
                    SortOrder         = Constant(ItemSortOrder.TopLevelItemDate)
                },
                AssertCount(2, "production-set:fullset"),
                CleanupCase
            );
        }
    }
}

}
