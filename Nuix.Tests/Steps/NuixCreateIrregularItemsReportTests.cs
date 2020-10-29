using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixCreateIrregularItemsReportTests : NuixStepTestBase<NuixCreateIrregularItemsReport, string>
    {
        /// <inheritdoc />
        public NuixCreateIrregularItemsReportTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get { yield break; }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { yield break; }

        }

    }
}