﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// The base of a ruby script step.
/// </summary>
public abstract class RubyScriptStepBase<T> : CompoundStep<T>, IRubyScriptStep<T>
{
    /// <summary>
    /// The string to use for the Nuix requirement
    /// </summary>
    public const string NuixRequirementName = "Nuix";

    /// <inheritdoc />
    public string FunctionName => RubyScriptStepFactory.RubyFunction.FunctionName;

    /// <inheritdoc />
    protected override Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) => RunAsync(stateMonad, cancellationToken);

    /// <summary>
    /// Runs this step asynchronously.
    /// </summary>
    protected async Task<Result<T, IError>> RunAsync(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var methodParameters = await TryGetMethodParameters(stateMonad, cancellationToken);

        if (methodParameters.IsFailure)
            return methodParameters.ConvertFailure<T>();

        var nuixConnection = stateMonad.GetOrCreateNuixConnection(false);

        if (nuixConnection.IsFailure)
            return nuixConnection.ConvertFailure<T>().MapError(x => x.WithLocation(this));

        var runResult = await nuixConnection.Value.RunFunctionAsync(
            stateMonad.Logger,
            RubyScriptStepFactory.RubyFunction,
            methodParameters.Value,
            cancellationToken
        );

        if (runResult.IsFailure &&
            runResult.Error.GetErrorBuilders()
                .Any(x => x.Data.TryPickT0(out var e, out _) && e is ChannelClosedException))
        {
            //The channel has closed on us. Try reopening it and rerunning the function

            nuixConnection = stateMonad.GetOrCreateNuixConnection(true);

            if (nuixConnection.IsFailure)
                return nuixConnection.ConvertFailure<T>().MapError(x => x.WithLocation(this));

            runResult = await nuixConnection.Value.RunFunctionAsync(
                stateMonad.Logger,
                RubyScriptStepFactory.RubyFunction,
                methodParameters.Value,
                cancellationToken
            );
        }

        return runResult.MapError(x => x.WithLocation(this));
    }

    /// <inheritdoc />
    public abstract IRubyScriptStepFactory<T> RubyScriptStepFactory { get; }

    /// <inheritdoc />
    public override IStepFactory StepFactory => RubyScriptStepFactory;

    /// <inheritdoc />
    public override IEnumerable<Requirement> RuntimeRequirements
    {
        get
        {
            var highestRequiredVersion = GetArgumentValues()
                .Keys
                .Select(x => x.RequiredNuixVersion)
                .Where(x => x != null)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (highestRequiredVersion != null)
            {
                yield return new Requirement
                {
                    MinVersion = highestRequiredVersion, Name = NuixRequirementName
                };
            }
        }
    }

    internal IReadOnlyDictionary<RubyFunctionParameter, IStep?> GetArgumentValues() =>
        RubyFunctionParameter.GetRubyFunctionArguments(this);

    internal async Task<Result<IReadOnlyDictionary<RubyFunctionParameter, object>, IError>>
        TryGetMethodParameters(IStateMonad stateMonad, CancellationToken cancellationToken)
    {
        var dict = new Dictionary<RubyFunctionParameter, object>();

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
                        ErrorHelper.MissingParameterError(argument.PropertyName)
                            .WithLocation(this)
                    );
            }
            else
            {
                if (errors.Any())
                    return Result
                        .Failure<IReadOnlyDictionary<RubyFunctionParameter, object>, IError>(
                            ErrorList.Combine(errors)
                        );
                //Don't try to evaluate argument if there are already errors

                var r = await process.Run<object>(stateMonad, cancellationToken);

                if (r.IsFailure)
                    errors.Add(r.Error);
                else
                {
                    object v = r.Value;
                    dict.Add(argument, v);
                }
            }
        }

        if (errors.Any())
            return Result.Failure<IReadOnlyDictionary<RubyFunctionParameter, object>, IError>(
                ErrorList.Combine(errors)
            );

        return dict;
    }
}

}
