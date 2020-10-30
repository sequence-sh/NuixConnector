using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixCreateIrregularItemsReportTests : NuixStepTestBase<NuixCreateIrregularItemsReport, string>
    {
        /// <inheritdoc />
        public NuixCreateIrregularItemsReportTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
                yield return new NuixIntegrationTestCase("Irregular Items",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new WriteFile
                    {
                        Text = new NuixCreateIrregularItemsReport
                        {
                            CasePath = CasePath
                        },
                        Folder = Constant(OutputFolder),
                        FileName = new Constant<string>("Irregular.txt")
                    },
                    AssertFileContains(OutputFolder, "Irregular.txt",
                        "Unrecognised\tNew Folder/data/Theme in Yellow.txt"),
                    AssertFileContains(OutputFolder, "Irregular.txt",
                        "NeedManualExamination\tNew Folder/data/Jellyfish.txt"),
                    DeleteCaseFolder,
                    DeleteOutputFolder
                );


            } }
    }
}