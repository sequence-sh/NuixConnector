using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixMigrateCaseTests : NuixStepTestBase<NuixMigrateCase, Unit>
    {
        /// <inheritdoc />
        public NuixMigrateCaseTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }


        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { yield break; }

        }

        /// <inheritdoc />
        protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases {
            get
            {
                yield return new NuixIntegrationTestCase("Migrate Case",
                    new DeleteItem {Path = Constants.MigrationTestCaseFolder},
                    new FileExtract
                    {
                        ArchiveFilePath = Constants.MigrationPath,
                        Destination = Constant(Constants.GeneralDataFolder)
                    },
                    new AssertError
                    {
                        Step = new NuixSearchAndTag
                        {
                            CasePath = Constants.MigrationTestCaseFolder, SearchTerm = Constant("*"),
                            Tag = Constant("item")
                        }
                    }, //This should fail because we can't open the case
                    new NuixCloseConnection(), //Not necessary
                    new NuixMigrateCase {CasePath = Constants.MigrationTestCaseFolder},
                    Constants.AssertCount(1, "jellyfish.txt", Constants.MigrationTestCaseFolder),
                    new NuixCloseConnection(),
                    new DeleteItem {Path = Constants.MigrationTestCaseFolder}
                );
            } }
    }
}