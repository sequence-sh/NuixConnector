using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests;

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
        public override async Task<StateMonad> GetStateMonad(
            IExternalContext externalContext,
            ILogger logger)
        {
            var externalProcessMock = new ExternalProcessMock(
                1,
                ExternalProcessActions.ToArray()
            );

            var newExternalContext = new ExternalContext(
                externalProcessMock,
                externalContext.RestClientFactory,
                externalContext.Console,
                externalContext.InjectedContexts
            );

            var baseMonad = await base.GetStateMonad(newExternalContext, logger);

            return new StateMonad(
                baseMonad.Logger,
                baseMonad.StepFactoryStore,
                newExternalContext,
                baseMonad.SequenceMetadata
            );
        }
    }
}
