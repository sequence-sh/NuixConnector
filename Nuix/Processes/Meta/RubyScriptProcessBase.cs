using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Conversion;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Processes.Meta
{
    /// <summary>
    /// The base of a ruby script process.
    /// </summary>
    public abstract class RubyScriptProcessBase<T> : CompoundRunnableProcess<T>, IRubyScriptProcess<T>
    {
        /// <summary>
        /// The string to use for the Nuix requirement
        /// </summary>
        public const string NuixProcessName = "Nuix";

        /// <inheritdoc />
        public string FunctionName => RubyScriptProcessFactory.RubyFunction.FunctionName;

        /// <inheritdoc />
        public abstract Result<string, IRunErrors> TryCompileScript(ProcessState processState);

        /// <inheritdoc />
        public override Result<T, IRunErrors> Run(ProcessState processState) => RunAsync(processState).Result;

        /// <summary>
        /// Runs this process asynchronously.
        /// </summary>
        protected abstract Task<Result<T, IRunErrors>> RunAsync(ProcessState processState);

        /// <inheritdoc />
        public abstract IRubyScriptProcessFactory<T> RubyScriptProcessFactory { get; }

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => RubyScriptProcessFactory;


        ///// <summary>
        ///// Required version of nuix, if it was changed by the parameters.
        ///// </summary>
        //public virtual Version? RunTimeNuixVersion => null;

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
                        Name = NuixProcessName
                    };
                }
            }
        }

        internal IReadOnlyDictionary<RubyFunctionParameter, IRunnableProcess?> GetArgumentValues() =>
            RubyFunctionParameter.GetRubyFunctionArguments(this);

        /// <inheritdoc />
        public abstract Result<IRubyBlock> TryConvert();


        internal Result<IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock>> TryGetArgumentsAsFunctions()
        {
            var dictionary = new Dictionary<RubyFunctionParameter, ITypedRubyBlock>();

            var values = GetArgumentValues();

            foreach (var rubyFunctionArgument in RubyScriptProcessFactory.RubyFunction.Arguments)
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


        internal Result<IReadOnlyDictionary<RubyFunctionParameter, string>, IRunErrors> TryGetMethodParameters(ProcessState processState)
        {
            var dict = new Dictionary<RubyFunctionParameter, string>();

            var argumentValues = GetArgumentValues();

            foreach (var argument in RubyScriptProcessFactory.RubyFunction.Arguments)
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
                        var r = process.Run<object>(processState);
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