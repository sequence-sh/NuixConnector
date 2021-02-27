using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Steps;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixCreateTermListTests : NuixStepTestBase<NuixCreateTermList, StringStream>
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
                "Create Term List",
                DeleteCaseFolder,
                DeleteOutputFolder,
                CreateOutputFolder,
                CreateCase,
                AddData,
                new FileWrite
                {
                    Stream = new NuixCreateTermList(),
                    Path   = new PathCombine { Paths = Array(OutputFolder, "Terms.txt") }
                },
                AssertFileContains(OutputFolder, "Terms.txt", "yellow	2"),
                new NuixCloseConnection(),
                DeleteCaseFolder,
                DeleteOutputFolder
            );
        }
    }
}

}
