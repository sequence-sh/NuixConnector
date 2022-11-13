namespace Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixOpenCaseTests : NuixStepTestBase<NuixOpenCase, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Test Open Case multiple times",
                DeleteCaseFolder,
                AssertCaseDoesNotExist,
                CreateCase,
                OpenCase,
                OpenCase,
                OpenCase,
                AssertCount(0, "*"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}
