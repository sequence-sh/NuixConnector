using System.IO;

namespace Reductech.Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixCreateNRTReportTests : NuixStepTestBase<NuixCreateNRTReport, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Export NRT Report",
                SetupCase,
                new DeleteItem { Path = NRTFolder },
                new NuixAddToProductionSet
                {
                    SearchTerm            = Constant("*.txt"),
                    ProductionSetName     = Constant("prodset"),
                    ProductionProfilePath = TestProductionProfilePath
                },
                new NuixCreateNRTReport
                {
                    CasePath = CasePath,
                    NRTPath =
                        Constant(Path.Join(Nuix8Path, @"user-data\Reports\Case Summary.nrt")),
                    OutputPath      = NRTFolder,
                    OutputFormat    = Constant("PDF"),
                    Title           = Constant("A report"),
                    User            = Constant("Investigator"),
                    ApplicationName = Constant("NuixApp")
                },
                AssertFileContains(GeneralDataFolder, "NRT", "PDF-1.4"),
                CleanupCase,
                new DeleteItem { Path = NRTFolder }
            );
        }
    }
}
