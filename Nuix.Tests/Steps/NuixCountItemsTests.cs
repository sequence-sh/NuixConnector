using Reductech.Sequence.Core.Steps;

namespace Reductech.Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixCountItemsTests : NuixStepTestBase<NuixCountItems, SCLInt>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Ingest and count items",
                SetupCase,
                new AssertTrue
                {
                    Boolean = new Equals<SCLInt>
                    {
                        Terms = new ArrayNew<SCLInt>
                        {
                            Elements = new List<IStep<SCLInt>>
                            {
                                Constant(1),
                                new NuixCountItems
                                {
                                    SearchTerm = Constant("jellyfish"),
                                    SearchOptions = Constant(
                                        Entity.Create(
                                            ("defaultFields", new[] { "name" })
                                        )
                                    )
                                }
                            }
                        }
                    }
                },
                CleanupCase
            );
        }
    }
}
