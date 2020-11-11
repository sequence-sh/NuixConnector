using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixCreateReportTests : NuixStepTestBase<NuixCreateReport, string>
    {
        /// <inheritdoc />
        public NuixCreateReportTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
                yield return new NuixIntegrationTestCase("Create Report",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new WriteFile
                    {
                        Text = new ToStream
                        {
                            Text = new NuixCreateReport
                            {
                                CasePath = CasePath,
                            }
                        } ,
                        Folder = Constant(OutputFolder),
                        FileName = new Constant<string>("Stats.txt")
                    },
                    AssertFileContains(OutputFolder, "Stats.txt", "Mark	type	text/plain	2"),

                    DeleteCaseFolder,
                    DeleteOutputFolder
                );


            } }
    }
}