using System;
using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

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
                "Test Open Case persistence after NuixCloseConnection",
                DeleteCaseFolder,
                AssertCaseDoesNotExist,
                CreateCase,
                OpenCase,
                AssertCount(0, "*.txt"),
                new NuixCloseConnection(),
                AssertCount(0, "*.txt"),
                new NuixCloseCase(),
                DeleteCaseFolder
            );
        }
    }
}

}
