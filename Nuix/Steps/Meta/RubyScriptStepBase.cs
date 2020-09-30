using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Conversion;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
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
        public abstract Result<string, IRunErrors> TryCompileScript(StateMonad stateMonad);

        /// <inheritdoc />
        public override Result<T, IRunErrors> Run(StateMonad stateMonad) => RunAsync(stateMonad).Result;

        /// <summary>
        /// Runs this step asynchronously.
        /// </summary>
        protected abstract Task<Result<T, IRunErrors>> RunAsync(StateMonad stateMonad);

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

        /// <inheritdoc />
        public abstract Result<IRubyBlock> TryConvert();


        internal Result<IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock>> TryGetArgumentsAsFunctions()
        {
            var dictionary = new Dictionary<RubyFunctionParameter, ITypedRubyBlock>();

            var values = GetArgumentValues();

            foreach (var rubyFunctionArgument in RubyScriptStepFactory.RubyFunction.Arguments)
            {
                if (values.TryGetValue(rubyFunctionArgument, out var rp) && rp != null)
                {
                    var br = RubyBlockConversion.TryConvert(rp, rubyFunctionArgument.ParameterName);

                    if (br.IsFailure)
                        return br.ConvertFailure<IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock>>(); //We can't convert this block - give up

                    if(br.Value is ITypedRubyBlock typedRubyBlock)
                        dictionary.Add(rubyFunctionArgument, typedRubyBlock);
                    else
                        return Result.Failure<IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock>>("Block was not typed"); //This will manifest as a proper error later

                }
                else if(rubyFunctionArgument.IsOptional)
                {
                    dictionary.Add(rubyFunctionArgument, new ConstantRubyBlock<string>(rubyFunctionArgument.ParameterName));
                }
            }

            return dictionary;
        }


        internal Result<IReadOnlyDictionary<RubyFunctionParameter, string>, IRunErrors> TryGetMethodParameters(StateMonad stateMonad)
        {
            var dict = new Dictionary<RubyFunctionParameter, string>();

            var argumentValues = GetArgumentValues();

            foreach (var argument in RubyScriptStepFactory.RubyFunction.Arguments)
            {
                if (argumentValues.TryGetValue(argument, out var process))
                {
                    if (process is null)
                    {
                        return Result.Failure<IReadOnlyDictionary<RubyFunctionParameter, string>, IRunErrors>(
                                    ErrorHelper.MissingParameterError(argument.ParameterName, Name));
                    }
                    else
                    {
                        var r = process.Run<object>(stateMonad);
                        if (r.IsFailure)
                            return r.ConvertFailure<IReadOnlyDictionary<RubyFunctionParameter, string>>();

                        var s = ConvertToString(r.Value);

                        dict.Add(argument, s);
                    }
                }
            }

            return dict;
        }

        private static string ConvertToString(object o)
        {
            if (o is int i)
                return i.ToString();
            if (o is bool b)
                return b.ToString().ToLower();
            if (o is Enum e)
                e.GetDescription();

            return o.ToString()!;

        }
    }
}