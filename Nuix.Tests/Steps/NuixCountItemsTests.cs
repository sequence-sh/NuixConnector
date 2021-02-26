using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixCountItemsTests : NuixStepTestBase<NuixCountItems, int>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases { get { yield break; } }
}

}
