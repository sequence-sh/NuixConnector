using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixGeneratePrintPreviewsTests : NuixStepTestBase<NuixGeneratePrintPreviews, Unit>
    {
        /// <inheritdoc />
        public NuixGeneratePrintPreviewsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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