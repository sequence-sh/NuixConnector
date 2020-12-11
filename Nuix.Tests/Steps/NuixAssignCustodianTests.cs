using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixAssignCustodianTests : NuixStepTestBase<NuixAssignCustodian, Unit>
    {
        /// <inheritdoc />
        public NuixAssignCustodianTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
                yield return new NuixIntegrationTestCase("Assign Custodians",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    AssertCount(0, "custodian:\"Jason\""),
                    new NuixAssignCustodian()
                        {CasePath = CasePath, Custodian = Constant("Jason"), SearchTerm = Constant("*")},
                    AssertCount(4, "custodian:\"Jason\""),
                    DeleteCaseFolder);


            } }
    }
}