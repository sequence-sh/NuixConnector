using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A ruby block that executes a function where every child function is another block.
    /// </summary>
    public abstract class CompoundRubyBlock : IRubyBlock
    {
        /// <summary>
        /// The final function to run.
        /// </summary>
        public abstract IRubyFunction Function { get; }

        /// <summary>
        /// Arguments to the function in the form of ruby blocks.
        /// </summary>
        public IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> Arguments { get; }

        /// <summary>
        /// Creates a new CompoundRubyBlock
        /// </summary>
        protected CompoundRubyBlock(
            IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> arguments) =>
            Arguments = arguments;


        /// <inheritdoc />
        public IEnumerable<IRubyFunction> FunctionDefinitions =>
            Arguments.SelectMany(x => x.Value.FunctionDefinitions).Append(Function);

        /// <inheritdoc />
        public Result<IReadOnlyCollection<string>, IRunErrors> TryGetArguments(Suffixer suffixer)
        {
            var resultsStrings = new List<string>();
            var errors = new List<IRunErrors>();

            foreach (var rubyFunctionArgument in Function.Arguments)
            {
                var childSuffixer = suffixer.GetNextChild();
                if (Arguments.TryGetValue(rubyFunctionArgument, out var block))
                {
                    var childResult = block.TryGetArguments(childSuffixer);

                    if (childResult.IsSuccess)
                        resultsStrings.AddRange(childResult.Value);
                    else
                        errors.Add(childResult.Error);
                }
                else if (!rubyFunctionArgument.IsOptional)
                    errors.Add(ErrorHelper.MissingParameterError(rubyFunctionArgument.ParameterName,
                        Function.FunctionName));

                //else do nothing, though note that the suffixer is incremented
            }

            if (errors.Any())
                return Result.Failure<IReadOnlyCollection<string>, IRunErrors>(RunErrorList.Combine(errors));

            return resultsStrings;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, Suffixer suffixer)
        {
            var r = new List<string>();

            foreach (var rubyFunctionArgument in Function.Arguments)
            {
                var childSuffixer = suffixer.GetNextChild();
                if (Arguments.TryGetValue(rubyFunctionArgument, out var block))
                    r.AddRange(block.GetOptParseLines(hashSetName, childSuffixer));
                //else assume the argument was optional, it will be nil later
            }

            return r;
        }

        internal Result<IReadOnlyCollection<string>, IRunErrors> GetArguments(Suffixer suffixer)
        {
            var argumentValues = new List<string>();
            var errors = new List<IRunErrors>();

            foreach (var rubyFunctionArgument in Function.Arguments)
            {
                var childSuffixer = suffixer.GetNextChild();
                if (Arguments.TryGetValue(rubyFunctionArgument, out var block))
                {
                    block.GetBlockText(childSuffixer, out var variableName);
                    argumentValues.Add(variableName);
                }
                else if (rubyFunctionArgument.IsOptional)
                    argumentValues.Add("nil");
                else
                    errors.Add(ErrorHelper.MissingParameterError(rubyFunctionArgument.ParameterName,
                        Function.FunctionName));
            }

            if (errors.Any())
                return Result.Failure<IReadOnlyCollection<string>, IRunErrors>(RunErrorList.Combine(errors));

            return argumentValues;
        }


    }

}