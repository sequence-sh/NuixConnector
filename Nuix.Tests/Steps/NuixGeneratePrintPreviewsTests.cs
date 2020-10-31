using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;


namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixGeneratePrintPreviewsTests : NuixStepTestBase<NuixGeneratePrintPreviews, Unit>
    {
        /// <inheritdoc />
        public NuixGeneratePrintPreviewsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
                yield return new NuixIntegrationTestCase("Generate Print Previews",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = Constant("*.txt"),
                        ProductionSetName = Constant("prodSet"),
                        ProductionProfilePath = TestProductionProfilePath
                    },
                    AssertCount(2, "production-set:prodSet"),
                    new NuixAssertPrintPreviewState
                    {
                        CasePath = CasePath,
                        ProductionSetName = Constant("prodSet"),
                        ExpectedState = Constant(PrintPreviewState.None)
                    },
                    new NuixGeneratePrintPreviews
                    {
                        CasePath = CasePath,
                        ProductionSetName = Constant("prodSet")
                    },
                    new NuixAssertPrintPreviewState
                    {
                        CasePath = CasePath,
                        ProductionSetName = Constant("prodSet"),
                        ExpectedState = Constant(PrintPreviewState.All)
                    },

                    DeleteCaseFolder);


            } }
    }
}