using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Steps;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public class
    NuixCreateIrregularItemsReportTests : NuixStepTestBase<NuixCreateIrregularItemsReport,
        StringStream>
{
    /// <inheritdoc />
    public NuixCreateIrregularItemsReportTests(ITestOutputHelper testOutputHelper) : base(
        testOutputHelper
    ) { }

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
                "Irregular Items",
                DeleteCaseFolder,
                DeleteOutputFolder,
                CreateOutputFolder,
                CreateCase,
                AddData,
                new FileWrite
                {
                    Stream = new NuixCreateIrregularItemsReport(),
                    Path   = new PathCombine() { Paths = Array(OutputFolder, "Irregular.txt") }
                },
                AssertFileContains(
                    OutputFolder,
                    "Irregular.txt",
                    "Unrecognised\tNew Folder/data/Theme in Yellow.txt"
                ),
                AssertFileContains(
                    OutputFolder,
                    "Irregular.txt",
                    "NeedManualExamination\tNew Folder/data/Jellyfish.txt"
                ),
                new NuixCloseConnection(),
                DeleteCaseFolder,
                DeleteOutputFolder
            );
        }
    }
}

}
