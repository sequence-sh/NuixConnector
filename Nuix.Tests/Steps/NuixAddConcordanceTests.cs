namespace Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixAddConcordanceTests : NuixStepTestBase<NuixAddConcordance, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Add concordance to case",
                DeleteCaseFolder,
                CreateCase,
                new NuixAddConcordance
                {
                    FilePath               = ConcordancePath,
                    Container              = Constant("ConcordanceTest"),
                    ConcordanceProfileName = Constant("IntegrationTestProfile"),
                    ConcordanceDateFormat  = Constant("yyyy-MM-dd'T'HH:mm:ss.SSSZ"),
                    Custodian              = Constant("Mark"),
                    Description            = Constant("Container description"),
                    ContainerEncoding      = Constant("UTF-8"),
                    ContainerLocale        = Constant("en-GB"),
                    ContainerTimeZone      = Constant("UTC")
                },
                AssertCount(3, "*"),
                AssertCount(1, "abandon"),
                CleanupCase
            );

            yield return new NuixIntegrationTestCase(
                "Add concordance to case with custom ProcessingSettings",
                DeleteCaseFolder,
                CreateCase,
                new NuixAddConcordance
                {
                    FilePath               = ConcordancePath,
                    Container              = Constant("ConcordanceTest"),
                    ConcordanceProfileName = Constant("IntegrationTestProfile"),
                    ContainerEncoding      = Constant("UTF-8"),
                    ProcessingSettings     = Constant(Entity.Create(("create_thumbnails", false))),
                    CustomMetadata         = Constant(Entity.Create(("CustomMeta", "value")))
                },
                AssertCount(3, "*"),
                AssertCount(1, "abandon"),
                AssertCount(3, "evidence-metadata:\"CustomMeta:*\""),
                CleanupCase
            );

            yield return new NuixIntegrationTestCase(
                "Add concordance to case with opticon file",
                DeleteCaseFolder,
                CreateCase,
                new NuixAddConcordance
                {
                    FilePath               = ConcordancePath,
                    Container              = Constant("ConcordanceTest"),
                    ConcordanceProfileName = Constant("IntegrationTestProfile"),
                    ContainerEncoding      = Constant("UTF-8"),
                    OpticonPath            = OpticonPath
                },
                AssertCount(3, "*"),
                AssertCount(1, "abandon"),
                CleanupCase
            );
        }
    }
}
