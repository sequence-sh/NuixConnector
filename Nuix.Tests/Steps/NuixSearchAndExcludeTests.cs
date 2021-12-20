using System.IO;

namespace Reductech.Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixSearchAndExcludeTests : NuixStepTestBase<NuixSearchAndExclude, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Search and Exclude",
                SetupCase,
                new NuixSearchAndExclude
                {
                    SearchTerm = Constant("charm"),
                    Tag        = Constant("exclude|charm"),
                    SortSearch = Constant(false)
                },
                AssertCount(1, "exclusion:charm"),
                AssertCount(1, "tag:\"exclude|charm\""),
                new NuixSearchAndExclude
                {
                    SearchTerm      = Constant("yellow"),
                    ExclusionReason = Constant("color"),
                    SortSearch      = Constant(true),
                    SearchOptions   = Constant(Entity.Create(("defaultFields", new[] { "name" })))
                },
                AssertCount(1, "exclusion:color"),
                AssertCount(0, "tag:yellow"),
                CleanupCase
            );

            yield return new NuixIntegrationTestCase(
                "Search and Exclude with SearchType",
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
                    Container = Constant("SearchAndExcludeTest")
                },
                new NuixSearchAndExclude
                {
                    SearchTerm      = Constant("name:\"msnbc.data.zip\""),
                    ExclusionReason = Constant("descendants"),
                    SearchType      = Constant(SearchType.Descendants)
                },
                AssertCount(3, "exclusion:descendants"),
                new NuixSearchAndExclude
                {
                    SearchTerm      = Constant("olympus"),
                    ExclusionReason = Constant("families"),
                    SearchType      = Constant(SearchType.Families)
                },
                AssertCount(1, "exclusion:families"),
                new NuixSearchAndExclude
                {
                    SearchTerm      = Constant("name:\"msnbc.data.zip\""),
                    ExclusionReason = Constant("itemsdescendants"),
                    SearchType      = Constant(SearchType.ItemsAndDescendants)
                },
                AssertCount(4, "exclusion:itemsdescendants"),
                new NuixSearchAndExclude
                {
                    SearchTerm      = Constant("name:\"description.txt\""),
                    ExclusionReason = Constant("itemsduplicates"),
                    SearchType      = Constant(SearchType.ItemsAndDuplicates)
                },
                AssertCount(2, "exclusion:itemsduplicates"),
                new NuixSearchAndExclude
                {
                    SearchTerm      = Constant("olympus"),
                    ExclusionReason = Constant("threads"),
                    SearchType      = Constant(SearchType.ThreadItems)
                },
                AssertCount(1, "exclusion:threads"),
                new NuixSearchAndExclude
                {
                    SearchTerm      = Constant("name:\"msnbc.html\""),
                    ExclusionReason = Constant("toplevel"),
                    SearchType      = Constant(SearchType.TopLevelItems)
                },
                AssertCount(1, "exclusion:toplevel"),
                CleanupCase
            );
        }
    }
}
