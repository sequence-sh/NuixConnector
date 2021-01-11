using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Steps;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public class NuixCreateReportTests : NuixStepTestBase<NuixCreateReport, StringStream>
{
    /// <inheritdoc />
    public NuixCreateReportTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

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
                "Create Report",
                DeleteCaseFolder,
                DeleteOutputFolder,
                CreateOutputFolder,
                CreateCase,
                AddData,
                new FileWrite
                {
                    Stream = new NuixCreateReport { CasePath = CasePath, },
                    Path   = new PathCombine { Paths         = Array(OutputFolder, "Stats.txt") }
                },
                AssertFileContains(OutputFolder, "Stats.txt", "Mark	type	text/plain	2"),
                DeleteCaseFolder,
                DeleteOutputFolder
            );
        }
    }
}

}
