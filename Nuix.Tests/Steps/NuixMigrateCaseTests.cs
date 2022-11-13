using Sequence.Core.Steps;

namespace Sequence.Connectors.Nuix.Tests.Steps;

public partial class NuixMigrateCaseTests : NuixStepTestBase<NuixMigrateCase, Unit>
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
                "Migrate Case",
                new DeleteItem { Path = Constants.MigrationTestCaseFolder },
                new FileExtract
                {
                    ArchiveFilePath = Constants.MigrationPath,
                    Destination     = Constant(Constants.GeneralDataFolder)
                },
                new AssertError
                {
                    Step = new NuixOpenCase { CasePath = Constants.MigrationTestCaseFolder }
                },                         //This should fail because we can't open the case
                new NuixCloseConnection(), //Not necessary
                new NuixMigrateCase { CasePath = Constants.MigrationTestCaseFolder },
                new NuixOpenCase { CasePath    = Constants.MigrationTestCaseFolder },
                Constants.AssertCount(1, "jellyfish.txt"),
                new NuixCloseConnection(),
                new DeleteItem { Path = Constants.MigrationTestCaseFolder }
            );
        }
    }
}
