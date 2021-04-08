using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Steps;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixCreateReportTests : NuixStepTestBase<NuixCreateReport, StringStream>
{
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
                SetupCase,
                DeleteOutputFolder,
                CreateOutputFolder,
                new FileWrite
                {
                    Stream = new NuixCreateReport(),
                    Path   = new PathCombine { Paths = Array(OutputFolder, "Stats.txt") }
                },
                AssertFileContains(OutputFolder, "Stats.txt", "Mark	type	text/plain	2"),
                CleanupCase,
                DeleteOutputFolder
            );
        }
    }
}

}
