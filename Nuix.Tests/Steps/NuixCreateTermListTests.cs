using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixCreateTermListTests : NuixStepTestBase<NuixCreateTermList, string>
    {
        /// <inheritdoc />
        public NuixCreateTermListTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
                yield return new NuixIntegrationTestCase("Create Term List",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new WriteFile
                    {
                        Text = new NuixCreateTermList
                        {
                            CasePath = CasePath,

                        },
                        Folder = Constant(OutputFolder),
                        FileName = new Constant<string>("Terms.txt")
                    }
                    ,
                    AssertFileContains(OutputFolder, "Terms.txt", "yellow	2"),

                    DeleteCaseFolder,
                    DeleteOutputFolder
                );


            } }
    }
}