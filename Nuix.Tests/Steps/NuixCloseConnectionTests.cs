using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public class NuixCloseConnectionTests : StepTestBase<NuixCloseConnection, Unit>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public NuixCloseConnectionTests([NotNull] ITestOutputHelper testOutputHelper) : base(
        testOutputHelper
    ) { }

    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Close connection that doesn't exist",
                new NuixCloseConnection(),
                Unit.Default
            );
        }
    }

    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield break;
        }
    }
}

}
