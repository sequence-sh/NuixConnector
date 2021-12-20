using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.Sequence.Core.Internal.Errors;

namespace Reductech.Sequence.Connectors.Nuix.Steps;

/// <summary>
/// Close the connection to nuix. Also closes all cases
/// </summary>
[Alias("NuixCloseCase")]
public sealed class NuixCloseConnection : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var r = await stateMonad.CloseNuixConnectionAsync(this, cancellationToken);

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
    public static SimpleStepFactory<NuixCloseConnection, Unit> Instance { get; } =
        new NuixCloseConnectionFactory();
}
