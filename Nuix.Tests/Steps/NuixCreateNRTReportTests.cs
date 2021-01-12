using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public class NuixCreateNRTReportTests : NuixStepTestBase<NuixCreateNRTReport, Unit>
{
    /// <inheritdoc />
    public NuixCreateNRTReportTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

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
                "Export NRT Report",
                DeleteCaseFolder,
                new DeleteItem { Path = NRTFolder },
                CreateCase,
                AddData,
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("*.txt"),
                    ProductionSetName     = Constant("prodset"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                new NuixCreateNRTReport
                {
                    CasePath = CasePath,
                    NRTPath = Constant(Path.Join(Nuix8Path, @"user-data\Reports\Case Summary.nrt")),
                    OutputFormat = Constant("PDF"),
                    LocalResourcesURL =
                        Constant(
                            Path.Join(Nuix8Path, @"user-data\Reports\Case Summary\Resources\")
                        ),
                    OutputPath = NRTFolder
                },
                AssertFileContains(GeneralDataFolder, "NRT", "PDF-1.4"),
                new DeleteItem { Path = NRTFolder },
                DeleteCaseFolder
            );
        }
    }
}

}
