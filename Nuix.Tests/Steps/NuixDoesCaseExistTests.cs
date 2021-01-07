using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public class NuixDoesCaseExistTests : NuixStepTestBase<NuixDoesCaseExist, bool>
{
    /// <inheritdoc />
    public NuixDoesCaseExistTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get { yield break; }
    }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases { get { yield break; } }
}

}
