using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
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
                    new FileWrite
                    {
                        Stream = new StringToStream
                        {
                            String = new NuixCreateReport
                            {
                                CasePath = CasePath,
                            }
                        } ,
                        Path = new PathCombine{Paths = new Constant<List<string>>(new List<string>(){OutputFolder,"Stats.txt" })}
                    },
                    AssertFileContains(OutputFolder, "Stats.txt", "Mark	type	text/plain	2"),

                    DeleteCaseFolder,
                    DeleteOutputFolder
                );


            } }
    }
}