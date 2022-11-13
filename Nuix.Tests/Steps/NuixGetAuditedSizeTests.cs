using Sequence.Core.Steps;

namespace Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixGetAuditedSizeTests : NuixStepTestBase<NuixGetAuditedSize, SCLDouble>
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
                    Boolean = new Equals<SCLDouble>
                    {
                        Terms = new ArrayNew<SCLDouble>
                        {
                            Elements = new List<IStep<SCLDouble>>
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
