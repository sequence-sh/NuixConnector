namespace Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixPerformOCRTests : NuixStepTestBase<NuixPerformOCR, Unit>
{
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
                "Perform OCR using Default profile",
                DeleteCaseFolder,
                CreateCase,
                new NuixAddItem
                {
                    Custodian = Constant("Mark"),
                    Paths     = PoemTextImagePaths,
                    Container = Constant("New Folder")
                },
                AssertCount(0, "sheep"),
                new NuixPerformOCR { SearchTerm = Constant("*.png") },
                AssertCount(1, "sheep"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );

            yield return new NuixIntegrationTestCase(
                "Perform OCR with named profile",
                DeleteCaseFolder,
                CreateCase,
                new NuixAddItem
                {
                    Custodian = Constant("Mark"),
                    Paths     = PoemTextImagePaths,
                    Container = Constant("New Folder")
                },
                AssertCount(0, "sheep OR ghost"),
                new NuixPerformOCR
                {
                    SearchTerm     = Constant("*.png"),
                    OCRProfileName = Constant("Default"),
                    SortSearch     = Constant(true),
                    SearchOptions  = Constant(Entity.Create(("limit", 1)))
                },
                AssertCount(1, "sheep OR ghost"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );

            yield return new NuixIntegrationTestCase(
                "Perform OCR with profile from path",
                DeleteCaseFolder,
                CreateCase,
                new NuixAddItem
                {
                    Custodian = Constant("Mark"),
                    Paths     = PoemTextImagePaths,
                    Container = Constant("New Folder")
                },
                AssertCount(0, "sheep OR ghost"),
                new NuixPerformOCR
                {
                    SearchTerm     = Constant("*.png"),
                    OCRProfilePath = DefaultOCRProfilePath,
                    SortSearch     = Constant(false)
                },
                AssertCount(2, "sheep OR ghost"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}
