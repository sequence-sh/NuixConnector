using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public  partial class NuixDoesCaseExistTests : NuixStepTestBase<NuixDoesCaseExist, bool>
{

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get { yield break; }
    }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases { get { yield break; } }
}

}
