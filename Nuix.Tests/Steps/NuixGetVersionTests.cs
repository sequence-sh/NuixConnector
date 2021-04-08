using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Steps;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixGetVersionTests : NuixStepTestBase<NuixGetVersion, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Return Nuix version",
                DeleteCaseFolder,
                CreateCase,
                new AssertTrue
                {
                    Boolean = new StringContains
                    {
                        String = new NuixGetVersion(),
                        Substring = new EntityGetValue<StringStream>
                        {
                            Entity   = new GetSettings(),
                            Property = Constant("Connectors.Nuix.Version")
                        }
                    }
                },
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
