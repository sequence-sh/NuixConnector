using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A ruby block where all the arguments are ruby functions.
    /// </summary>
    public sealed class TypedCompoundRubyBlock<T> : CompoundRubyBlock, ITypedRubyBlock<T>
    {
        /// <inheritdoc />
        public TypedCompoundRubyBlock(IRubyFunction<T> typedFunction, IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> arguments) :
            base(arguments) =>
            TypedFunction = typedFunction;

        /// <summary>
        /// The final function to run.
        /// </summary>
        public IRubyFunction<T> TypedFunction { get; }

        /// <inheritdoc />
        public override IRubyFunction Function => TypedFunction;

        /// <inheritdoc />
        public Result<string, IRunErrors> GetBlockText(Suffixer suffixer, out string resultVariableName)
        {
            var stringBuilder = new StringBuilder();

            var arguments = new List<string>();
            var errors = new List<IRunErrors>();
            resultVariableName = Function.FunctionName + suffixer.CurrentSuffix;


            foreach (var rubyFunctionArgument in Function.Arguments)
            {
                var childSuffixer = suffixer.GetNextChild();
                if (Arguments.TryGetValue(rubyFunctionArgument, out var block))
                {
                    var childResult = block.GetBlockText(childSuffixer, out var argument);
                    arguments.Add(argument);

                    if (childResult.IsSuccess)
                        stringBuilder.AppendLine(childResult.Value);
                    else
                        errors.Add(childResult.Error);
                }
                else if (!rubyFunctionArgument.IsOptional)
                    errors.Add(ErrorHelper.MissingParameterError(rubyFunctionArgument.ParameterName,
                        Function.FunctionName));
            }

            if (errors.Any())
            {
                return RunErrorList.Combine(errors);
            }

            stringBuilder.Append($"{resultVariableName} = {Function.FunctionName}(");
            stringBuilder.AppendJoin(", ", arguments);
            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }
    }
}