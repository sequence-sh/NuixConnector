namespace Reductech.EDR.Connectors.Nuix.Tests.Steps;

public partial class NuixExtractEntitiesTests : NuixStepTestBase<NuixExtractEntities, Unit>
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
                "Extract Entities",
                DeleteCaseFolder,
                DeleteOutputFolder,
                CreateOutputFolder,
                CreateCase,
                //Note - we have to add items with a special profile in order to extract entities
                new NuixAddItem
                {
                    Custodian             = Constant("Mark"),
                    Paths                 = DataPaths,
                    Container             = Constant("New Folder"),
                    ProcessingProfileName = Constant("ExtractEntities")
                },
                new NuixExtractEntities { OutputFolder = Constant(OutputFolder) },
                AssertFileContains(OutputFolder, "email.txt", "Marianne.Moore@yahoo.com"),
                new NuixCloseConnection(),
                DeleteCaseFolder,
                DeleteOutputFolder
            );
        }
    }
}
