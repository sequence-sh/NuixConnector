﻿namespace Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixDoesCaseExistTests : NuixStepTestBase<NuixDoesCaseExist, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get { yield break; }
    }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases { get { yield break; } }
}
