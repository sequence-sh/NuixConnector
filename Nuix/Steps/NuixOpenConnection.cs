using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.Sequence.Core.Internal.Errors;

namespace Reductech.Sequence.Connectors.Nuix.Steps;

/// <summary>
/// Open the connection to nuix.
/// Does not open the case.
/// </summary>
public sealed class NuixOpenConnection : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var r = await stateMonad.GetOrCreateNuixConnection(this, false);

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
