using System.Collections.Generic;
using System.Linq;
using Moq;
using OneOf;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Steps;
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

            AddFileSystemAction(
                x => x.Setup(f => f.DoesFileExist(It.IsAny<string>())).Returns(true)
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

            AddFileSystemAction(
                x => x.Setup(f => f.DoesFileExist(It.IsAny<string>())).Returns(true)
            );
        }

        public IReadOnlyCollection<ExternalProcessAction> ExternalProcessActions { get; }

        /// <inheritdoc />
        public override IExternalProcessRunner GetExternalProcessRunner(
            MockRepository mockRepository) =>
            new ExternalProcessMock(1, ExternalProcessActions.ToArray());
    }
}

}
