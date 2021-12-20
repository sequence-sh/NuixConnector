namespace Reductech.Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixImportDocumentIdsTests : NuixStepTestBase<NuixImportDocumentIds, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get { yield break; }
    }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases { get { yield break; } }
}
