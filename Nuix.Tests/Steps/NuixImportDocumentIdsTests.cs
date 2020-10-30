using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixImportDocumentIdsTests : NuixStepTestBase<NuixImportDocumentIds, Unit>
    {
        /// <inheritdoc />
        public NuixImportDocumentIdsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { yield break; }

        }

        /// <inheritdoc />
        protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases { get { yield break; } }

    }
}