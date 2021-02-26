using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class
    NuixRemoveFromProductionSetTests : NuixStepTestBase<NuixRemoveFromProductionSet, Unit>
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
                "Remove From Production Set",
                DeleteCaseFolder,
                CreateCase,
                AddData,
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("*.txt"),
                    ProductionSetName     = Constant("fullset"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                new NuixRemoveFromProductionSet
                {
                    SearchTerm = Constant("Charm"), ProductionSetName = Constant("fullset")
                },
                AssertCount(1, "production-set:fullset"),
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("\"and\""),
                    ProductionSetName     = Constant("conjunction"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                new NuixRemoveFromProductionSet
                {
                    ProductionSetName = Constant("conjunction"),
                    SearchTerm        = Constant("jellyfish"),
                    SearchOptions = Constant(
                        Core.Entity.Create(("defaultFields", new[] { "name" }))
                    )
                },
                AssertCount(1, "production-set:conjunction"),
                new NuixRemoveFromProductionSet { ProductionSetName = Constant("conjunction") },
                AssertCount(0, "production-set:conjunction"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
