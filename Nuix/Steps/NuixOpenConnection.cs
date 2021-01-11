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
[Alias("NuixOpenCase")]
public sealed class NuixOpenConnection : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var r = stateMonad.GetOrCreateNuixConnection(false);

        if (r.IsFailure)
            return r.MapError(x => x.WithLocation(this)).ConvertFailure<Unit>();

        return Unit.Default;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => NuixOpenConnectionFactory.Instance;
}

/// <summary>
/// Close the connection to nuix
/// </summary>
public sealed class NuixOpenConnectionFactory : SimpleStepFactory<NuixOpenConnection, Unit>
{
    private NuixOpenConnectionFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static SimpleStepFactory<NuixOpenConnection, Unit> Instance { get; } =
        new NuixOpenConnectionFactory();
}

}
