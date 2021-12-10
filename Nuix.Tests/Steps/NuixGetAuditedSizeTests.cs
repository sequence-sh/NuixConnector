using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixGetAuditedSizeTests : NuixStepTestBase<NuixGetAuditedSize, double>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Ingest items and calculate audited size",
                DeleteCaseFolder,
                CreateCase,
                new NuixAddItem
                {
                    Custodian          = Constant("Mark"),
                    Paths              = DataPaths,
                    Container          = Constant("New Folder"),
                    ProcessingSettings = Constant(Entity.Create(("calculateAuditedSize", true)))
                },
                new AssertTrue
                {
                    Boolean = new Equals<double>
                    {
                        Terms = new ArrayNew<double>
                        {
                            Elements = new List<IStep<double>>
                            {
                                Constant(799.0), new NuixGetAuditedSize()
                            }
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
