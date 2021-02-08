using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

public abstract partial class NuixStepTestBase<TStep, TOutput>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases { get { yield break; } }

    public record NuixStepCase : StepCase
    {
        public NuixStepCase(
            string name,
            Sequence<TOutput> sequence,
            IReadOnlyCollection<ExternalProcessAction> externalProcessActions,
            params string[] expectedLogValues)
            : base(
                name,
                sequence,
                new ExpectedOutput(OneOf<Unit, TOutput>.FromT0(Unit.Default)),
                expectedLogValues
            )
        {
            ExternalProcessActions = externalProcessActions;
            IgnoreFinalState       = true;

            this.WithFileAction(x => x.Setup(f => f.Exists(It.IsAny<string>())).Returns(true));
        }

        public NuixStepCase(
            string name,
            TStep step,
            TOutput expectedOutput,
            IReadOnlyCollection<ExternalProcessAction> externalProcessActions,
            params string[] expectedLogValues)
            : base(name, step, expectedOutput, expectedLogValues)
        {
            ExternalProcessActions = externalProcessActions;
            IgnoreFinalState       = true;

            this.WithFileAction(x => x.Setup(f => f.Exists(It.IsAny<string>())).Returns(true));
        }

        public IReadOnlyCollection<ExternalProcessAction> ExternalProcessActions { get; }

        /// <inheritdoc />
        public override StateMonad GetStateMonad(MockRepository mockRepository, ILogger logger)
        {
            var baseMonad = base.GetStateMonad(mockRepository, logger);

            var externalProcessMock = new ExternalProcessMock(1, ExternalProcessActions.ToArray());

            return new StateMonad(
                baseMonad.Logger,
                baseMonad.Settings,
                baseMonad.StepFactoryStore,
                new ExternalContext(
                    baseMonad.ExternalContext.FileSystemHelper,
                    externalProcessMock,
                    baseMonad.ExternalContext.Console
                )
            );
        }
    }
}

}
