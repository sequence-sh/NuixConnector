using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Close the connection to nuix
    /// </summary>
    [Alias("NuixCloseCase")]
    public sealed class NuixCloseConnection : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var r = await stateMonad.CloseNuixConnectionAsync(cancellationToken);

            return r.MapError(x => x.WithLocation(this));
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => NuixCloseConnectionFactory.Instance;
    }

    /// <summary>
    /// Close the connection to nuix
    /// </summary>
    public sealed class NuixCloseConnectionFactory : SimpleStepFactory<NuixCloseConnection, Unit>
    {
        private NuixCloseConnectionFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<NuixCloseConnection, Unit> Instance { get; } = new NuixCloseConnectionFactory();
    }
}
