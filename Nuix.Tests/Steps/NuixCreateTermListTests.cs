using Reductech.Sequence.Connectors.FileSystem;

namespace Reductech.Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixCreateTermListTests : NuixStepTestBase<NuixCreateTermList, StringStream>
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
                "Create Term List",
                SetupCase,
                DeleteOutputFolder,
                CreateOutputFolder,
                new FileWrite
                {
                    Stream = new NuixCreateTermList(),
                    Path   = new PathCombine { Paths = Array(OutputFolder, "Terms.txt") }
                },
                AssertFileContains(OutputFolder, "Terms.txt", "yellow	2"),
                CleanupCase,
                DeleteOutputFolder
            );
        }
    }
}
