using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public  partial class NuixReorderProductionSetTests : NuixStepTestBase<NuixReorderProductionSet, Unit>
{

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases { get { yield break; } }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases { get { yield break; } }
}

}
