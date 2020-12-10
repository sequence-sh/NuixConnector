using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public abstract partial class NuixStepTestBase<TStep, TOutput>
    {
        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases { get { yield break; } }



        public class NuixStepCase : StepCase
        {

            public NuixStepCase(string name,
                Sequence sequence,
                IReadOnlyCollection<ExternalProcessAction> externalProcessActions,
                params string[] expectedLogValues)
                : base(name, sequence, Maybe<TOutput>.None, expectedLogValues)
            {
                ExternalProcessActions = externalProcessActions;
                IgnoreFinalState = true;
                AddFileSystemAction(x => x.Setup(f => f.DoesFileExist(It.IsAny<string>())).Returns(true));
            }

            public NuixStepCase(string name,
                TStep step,
                TOutput expectedOutput,
                IReadOnlyCollection<ExternalProcessAction> externalProcessActions, params string[] expectedLogValues)
                : base(name, step, expectedOutput, expectedLogValues)
            {
                ExternalProcessActions = externalProcessActions;
                IgnoreFinalState = true;
                AddFileSystemAction(x => x.Setup(f => f.DoesFileExist(It.IsAny<string>())).Returns(true));
            }

            public IReadOnlyCollection<ExternalProcessAction> ExternalProcessActions { get; }

            /// <inheritdoc />
            public override IExternalProcessRunner GetExternalProcessRunner(MockRepository mockRepository) => new ExternalProcessMock(1, ExternalProcessActions.ToArray());
        }
    }
}
