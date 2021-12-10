using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps;

public partial class NuixCountItemsTests : NuixStepTestBase<NuixCountItems, int>
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
                    Boolean = new Equals<int>
                    {
                        Terms = new ArrayNew<int>
                        {
                            Elements = new List<IStep<int>>
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
