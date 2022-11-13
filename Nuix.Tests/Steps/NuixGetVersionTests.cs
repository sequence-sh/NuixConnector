using Sequence.Core.Steps;

namespace Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixGetVersionTests : NuixStepTestBase<NuixGetVersion, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Return Nuix version",
                DeleteCaseFolder,
                CreateCase,
                new AssertTrue
                {
                    Boolean = new StringContains
                    {
                        String = new NuixGetVersion(),
                        Substring = new EntityGetValue<StringStream>
                        {
                            Entity   = new GetSettings(),
                            Property = Constant("Connectors.Nuix.Version")
                        }
                    }
                },
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}
