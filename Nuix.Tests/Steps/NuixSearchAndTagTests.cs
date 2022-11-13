using System.IO;

namespace Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixSearchAndTagTests : NuixStepTestBase<NuixSearchAndTag, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Search and Tag",
                SetupCase,
                new NuixSearchAndTag { SearchTerm = Constant("charm"), Tag = Constant("charm") },
                AssertCount(1, "tag:charm"),
                new NuixSearchAndTag
                {
                    SearchTerm    = Constant("\"and\""),
                    Tag           = Constant("conjunction"),
                    SortSearch    = Constant(true),
                    SearchOptions = Constant(Entity.Create(("limit", 1)))
                },
                AssertCount(1, "tag:conjunction"),
                CleanupCase
            );

            yield return new NuixIntegrationTestCase(
                "Search and Tag with SearchType",
                DeleteCaseFolder,
                CreateCase,
                new NuixAddItem
                {
                    Custodian = Constant("Mark"),
                    Paths = Array(
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "AllData",
                            "descendants.zip"
                        )
                    ),
                    Container = Constant("SearchAndTagTest")
                },
                new NuixSearchAndTag
                {
                    SearchTerm = Constant("name:\"msnbc.data.zip\""),
                    Tag        = Constant("descendants"),
                    SearchType = Constant(SearchType.Descendants)
                },
                AssertCount(3, "tag:descendants"),
                new NuixSearchAndTag
                {
                    SearchTerm = Constant("olympus"),
                    Tag        = Constant("families"),
                    SearchType = Constant(SearchType.Families)
                },
                AssertCount(1, "tag:families"),
                new NuixSearchAndTag
                {
                    SearchTerm = Constant("name:\"msnbc.data.zip\""),
                    Tag        = Constant("itemsdescendants"),
                    SearchType = Constant(SearchType.ItemsAndDescendants)
                },
                AssertCount(4, "tag:itemsdescendants"),
                new NuixSearchAndTag
                {
                    SearchTerm = Constant("name:\"description.txt\""),
                    Tag        = Constant("itemsduplicates"),
                    SearchType = Constant(SearchType.ItemsAndDuplicates)
                },
                AssertCount(2, "tag:itemsduplicates"),
                new NuixSearchAndTag
                {
                    SearchTerm = Constant("olympus"),
                    Tag        = Constant("threads"),
                    SearchType = Constant(SearchType.ThreadItems)
                },
                AssertCount(1, "tag:threads"),
                new NuixSearchAndTag
                {
                    SearchTerm = Constant("name:\"msnbc.html\""),
                    Tag        = Constant("toplevel"),
                    SearchType = Constant(SearchType.TopLevelItems)
                },
                AssertCount(1, "tag:toplevel"),
                CleanupCase
            );
        }
    }
}
