using System.Collections.Generic;
using System.Linq;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

public abstract partial class NuixStepTestBase<TStep, TOutput>
{
    public record NuixDeserializeTest : DeserializeCase
    {
        /// <inheritdoc />
        public NuixDeserializeTest(
            string name,
            string yaml,
            TOutput expectedOutput,
            IReadOnlyCollection<ExternalProcessAction> externalProcessActions,
            params string[] expectedLoggedValues) : base(
            name,
            yaml,
            expectedOutput,
            expectedLoggedValues
        )
        {
            ExternalProcessActions = externalProcessActions;
            IgnoreFinalState       = true;
        }

        /// <inheritdoc />
        public NuixDeserializeTest(
            string name,
            string yaml,
            Unit _,
            IReadOnlyCollection<ExternalProcessAction> externalProcessActions,
            params string[] expectedLoggedValues) : base(
            name,
            yaml,
            _,
            expectedLoggedValues
        )
        {
            ExternalProcessActions = externalProcessActions;
            IgnoreFinalState       = true;
        }

        public IReadOnlyCollection<ExternalProcessAction> ExternalProcessActions { get; }

        /// <inheritdoc />
        public override IExternalProcessRunner GetExternalProcessRunner(
            MockRepository mockRepository) =>
            new ExternalProcessMock(1, ExternalProcessActions.ToArray());
    }
}

}
