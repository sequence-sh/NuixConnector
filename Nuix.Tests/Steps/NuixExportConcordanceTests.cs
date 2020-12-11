using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixExportConcordanceTests : NuixStepTestBase<NuixExportConcordance, Unit>
    {
        /// <inheritdoc />
        public NuixExportConcordanceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

                yield return new NuixIntegrationTestCase("Export Concordance",
                    DeleteCaseFolder,
                    new DeleteItem {Path = Constant(ConcordanceFolder)},
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = Constant("charm"),
                        ProductionSetName = Constant("charmset"),
                        ProductionProfilePath = TestProductionProfilePath
                    },
                    new NuixExportConcordance
                    {
                        CasePath = CasePath,
                        ProductionSetName = Constant("charmset"),
                        ExportPath = Constant(ConcordanceFolder)
                    },
                    AssertFileContains(ConcordanceFolder, "loadfile.dat", "DOCID"),
                    AssertFileContains(ConcordanceFolder, "TEXT/000/000/DOC-000000001.txt", "Visible, invisible"),
                    new DeleteItem {Path =  Constant(ConcordanceFolder)},
                    DeleteCaseFolder
                );

            } }
    }
}