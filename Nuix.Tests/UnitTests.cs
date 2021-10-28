using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

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
                ExpectedUnitOutput.Instance,
                expectedLogValues
            )
        {
            ExternalProcessActions = externalProcessActions;
            IgnoreFinalState       = true;

            this.WithContextMock(
                ConnectorInjection.FileSystemKey,
                mr =>
                {
                    var mock = mr.Create<IFileSystem>();

                    mock.Setup(f => f.File.Exists(It.IsAny<string>()))
                        .Returns(true);

                    return mock;
                }
            );
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

            this.WithContextMock(
                ConnectorInjection.FileSystemKey,
                mr =>
                {
                    var mock = mr.Create<IFileSystem>();

                    mock.Setup(f => f.File.Exists(It.IsAny<string>()))
                        .Returns(true);

                    return mock;
                }
            );
        }

        public IReadOnlyCollection<ExternalProcessAction> ExternalProcessActions { get; }

        /// <inheritdoc />
        public override async Task<StateMonad> GetStateMonad(
            MockRepository mockRepository,
            ILogger logger)
        {
            var baseMonad = await base.GetStateMonad(mockRepository, logger);

            var externalProcessMock = new ExternalProcessMock(
                1,
                ExternalProcessActions.ToArray()
            );

            var stepClient = RESTClientSetupHelper.GetRESTClient(mockRepository, FinalChecks);

            return new StateMonad(
                baseMonad.Logger,
                baseMonad.StepFactoryStore,
                new ExternalContext(
                    externalProcessMock,
                    baseMonad.ExternalContext.Console,
                    baseMonad.ExternalContext.InjectedContexts
                ),
                new SingleRestClientFactory(stepClient),
                baseMonad.SequenceMetadata
            );
        }
    }
}

}
