using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixCreateNRTReportTests : NuixStepTestBase<NuixCreateNRTReport, Unit>
{
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
                    NRTPath =
                        Constant(Path.Join(Nuix8Path, @"user-data\Reports\Case Summary.nrt")),
                    OutputPath      = NRTFolder,
                    OutputFormat    = Constant("PDF"),
                    Title           = Constant("A report"),
                    User            = Constant("Investigator"),
                    ApplicationName = Constant("NuixApp")
                },
                AssertFileContains(GeneralDataFolder, "NRT", "PDF-1.4"),
                new NuixCloseConnection(),
                new DeleteItem { Path = NRTFolder },
                DeleteCaseFolder
            );
        }
    }
}

}
