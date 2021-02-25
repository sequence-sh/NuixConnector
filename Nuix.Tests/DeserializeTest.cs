using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Abstractions;
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
        public override async Task<StateMonad> GetStateMonad(
            MockRepository mockRepository,
            ILogger logger)
        {
            var baseMonad = await base.GetStateMonad(mockRepository, logger);

            var externalProcessMock = new ExternalProcessMock(1, ExternalProcessActions.ToArray());

            return new StateMonad(
                baseMonad.Logger,
                baseMonad.Settings,
                baseMonad.StepFactoryStore,
                new ExternalContext(
                    baseMonad.ExternalContext.FileSystemHelper,
                    externalProcessMock,
                    baseMonad.ExternalContext.Console
                ),
                baseMonad.SequenceMetadata
            );
        }
    }
}

}
