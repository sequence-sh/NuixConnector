using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Sequence.Core.Internal.Errors;

namespace Sequence.Connectors.Nuix.Steps.Meta;

/// <summary>
/// The base of a ruby script step.
/// </summary>
public abstract class RubyScriptStepBase<T> : CompoundStep<T>, IRubyScriptStep<T>
    where T : ISCLObject
{
    /// <summary>
    ///The name of the Nuix Connector
    /// </summary>
    public const string NuixFeaturesKey = "Features";

    /// <summary>
    /// Key to the settings version property
    /// </summary>
    public const string NuixVersionKey = "Version";

    /// <inheritdoc />
    public string FunctionName => RubyScriptStepFactory.RubyFunction.FunctionName;

    /// <inheritdoc />
    protected override ValueTask<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) => RunAsync(stateMonad, cancellationToken);

    /// <summary>
    /// Gets the CasePath Parameter.
    /// </summary>
    public abstract CasePathParameter CasePathParameter { get; }

    /// <inheritdoc />
    public override Result<Unit, IError> VerifyThis(StepFactoryStore stepFactoryStore)
    {
        var settings = StateMonad.CreateSettingsEntity(stepFactoryStore);

        var r = SettingsHelpers.TryGetNuixSettings(settings)
            .Bind(NuixConnectionHelper.TryGetConsoleArguments)
            .MapError(x => x.WithLocation(this));

        if (r.IsFailure)
            return r.ConvertFailure<Unit>();

        return base.VerifyThis(stepFactoryStore);
    }

    /// <summary>
    /// Runs this step asynchronously.
    /// </summary>
    protected async ValueTask<Result<T, IError>> RunAsync(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var methodParameters = await TryGetMethodParameters(stateMonad, cancellationToken);

        if (methodParameters.IsFailure)
            return methodParameters.ConvertFailure<T>();

        var nuixConnection = await stateMonad.GetOrCreateNuixConnection(this, false);

        if (nuixConnection.IsFailure)
            return nuixConnection.ConvertFailure<T>().MapError(x => x.WithLocation(this));

        var runResult = await nuixConnection.Value.RunFunctionAsync(
            stateMonad,
            this,
            RubyScriptStepFactory.RubyFunction,
            methodParameters.Value,
            CasePathParameter,
            cancellationToken
        );

        if (runResult.IsFailure &&
            runResult.Error.GetErrorBuilders()
                .Any(x => x.Data is ErrorData.ExceptionData { Exception: ChannelClosedException }))
        {
            //The channel has closed on us. Try reopening it and rerunning the function

            nuixConnection = await stateMonad.GetOrCreateNuixConnection(this, true);

            if (nuixConnection.IsFailure)
                return nuixConnection.ConvertFailure<T>().MapError(x => x.WithLocation(this));

            runResult = await nuixConnection.Value.RunFunctionAsync(
                stateMonad,
                this,
                RubyScriptStepFactory.RubyFunction,
                methodParameters.Value,
                CasePathParameter,
                cancellationToken
            );
        }

        return runResult.MapError(x => x.WithLocation(this));
    }

    /// <inheritdoc />
    public abstract IRubyScriptStepFactory<T> RubyScriptStepFactory { get; }

    /// <inheritdoc />
    public override IStepFactory StepFactory => RubyScriptStepFactory;

    internal IReadOnlyDictionary<RubyFunctionParameter, IStep?> GetArgumentValues() =>
        RubyFunctionParameter.GetRubyFunctionArguments(this);

    internal async Task<Result<IReadOnlyDictionary<RubyFunctionParameter, ISCLObject>, IError>>
        TryGetMethodParameters(IStateMonad stateMonad, CancellationToken cancellationToken)
    {
        var dict = new Dictionary<RubyFunctionParameter, ISCLObject>();

        var errors = new List<IError>();

        var argumentValues = GetArgumentValues();

        foreach (var argument in RubyScriptStepFactory.RubyFunction.Arguments)
        {
            if (!argumentValues.TryGetValue(argument, out var process))
                continue;

            if (process is null)
            {
                if (!argument.IsOptional)
                    errors.Add(
                        ErrorCode.MissingParameter.ToErrorBuilder(argument.PropertyName)
                            .WithLocation(this)
                    );
            }
            else
            {
                if (errors.Any())
                    return Result
                        .Failure<IReadOnlyDictionary<RubyFunctionParameter, ISCLObject>, IError>(
                            ErrorList.Combine(errors)
                        );
                //Don't try to evaluate argument if there are already errors

                var r = await process.RunUntyped(stateMonad, cancellationToken);

                if (r.IsFailure)
                    errors.Add(r.Error);
                else
                {
                    dict.Add(argument, r.Value);
                }
            }
        }

        if (errors.Any())
            return Result.Failure<IReadOnlyDictionary<RubyFunctionParameter, ISCLObject>, IError>(
                ErrorList.Combine(errors)
            );

        return dict;
    }
}
