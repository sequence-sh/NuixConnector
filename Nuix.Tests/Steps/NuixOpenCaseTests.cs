using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public class NuixOpenCaseTests : NuixStepTestBase<NuixOpenCase, Unit>
{
    /// <inheritdoc />
    public NuixOpenCaseTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Test Open Case multiple times",
                DeleteCaseFolder,
                AssertCaseDoesNotExist,
                CreateCase,
                OpenCase,
                OpenCase,
                OpenCase,
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
