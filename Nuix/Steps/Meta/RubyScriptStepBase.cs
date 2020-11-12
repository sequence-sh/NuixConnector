using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

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
        public override Task<Result<T, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken) => RunAsync(stateMonad, cancellationToken);

        /// <summary>
        /// Runs this step asynchronously.
        /// </summary>
        protected async Task<Result<T, IError>> RunAsync(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var nuixConnection = stateMonad.GetOrCreateNuixConnection();

            if (nuixConnection.IsFailure) return nuixConnection.ConvertFailure<T>().MapError(x=>x.WithLocation(this));

            var methodParameters = await TryGetMethodParameters(stateMonad, cancellationToken);

            if (methodParameters.IsFailure) return methodParameters.ConvertFailure<T>();

            var runResult = await nuixConnection.Value.RunFunctionAsync(stateMonad.Logger, RubyScriptStepFactory.RubyFunction,
                methodParameters.Value, cancellationToken);

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
                var highestRequiredVersion = GetArgumentValues().Keys
                    .Select(x => x.RequiredNuixVersion)
                    .Where(x=>x != null)
                    .OrderByDescending(x=>x)
                    .FirstOrDefault();

                if (highestRequiredVersion != null)
                {
                    yield return new Requirement
                    {
                        MinVersion = highestRequiredVersion,
                        Name = NuixRequirementName
                    };
                }
            }
        }

        internal IReadOnlyDictionary<RubyFunctionParameter, IStep?> GetArgumentValues() =>
            RubyFunctionParameter.GetRubyFunctionArguments(this);




        internal async Task<Result<IReadOnlyDictionary<RubyFunctionParameter, object>, IError>>
            TryGetMethodParameters(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var dict = new Dictionary<RubyFunctionParameter, object>();

            var argumentValues = GetArgumentValues();

            foreach (var argument in RubyScriptStepFactory.RubyFunction.Arguments)
            {
                if (!argumentValues.TryGetValue(argument, out var process)) continue;

                if (process is null)
                {
                    if (!argument.IsOptional)
                        return Result.Failure<IReadOnlyDictionary<RubyFunctionParameter, object>, IError>(
                            ErrorHelper.MissingParameterError(argument.ParameterName, Name).WithLocation(this));
                }
                else
                {
                    var r = await process.Run<object>(stateMonad, cancellationToken);
                    if (r.IsFailure)
                        return r.ConvertFailure<IReadOnlyDictionary<RubyFunctionParameter, object>>();

                    dict.Add(argument, r.Value);
                }
            }

            return dict;
        }
    }
}