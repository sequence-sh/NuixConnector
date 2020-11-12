using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
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
        public Result<string, IErrorBuilder> TryWriteBlockLines(Suffixer suffixer, IIndentationStringBuilder stringBuilder)
        {
            var arguments = new List<string>();
            var errors = new List<IErrorBuilder>();
            var resultVariableName = Function.FunctionName.ToLowerInvariant() + suffixer.CurrentSuffix;

            if (Function.RequireUtilities)
                arguments.Add(RubyScriptCompilationHelper.UtilitiesParameterName);

            var childSuffixer = suffixer.GetNextChild();

            foreach (var rubyFunctionArgument in Function.Arguments)
            {

                if (Arguments.TryGetValue(rubyFunctionArgument, out var block))
                {
                    var argumentResult = block.TryWriteBlockLines(childSuffixer, stringBuilder);

                    if (argumentResult.IsSuccess)
                        arguments.Add(argumentResult.Value);
                    else
                        errors.Add(argumentResult.Error);
                }
                else
                {
                    childSuffixer.GetNext();//increment sudffixer anyway
                    if (!rubyFunctionArgument.IsOptional)
                    {
                        errors.Add(ErrorHelper.MissingParameterError(rubyFunctionArgument.ParameterName,
                            Function.FunctionName));
                    }
                }
            }

            if (errors.Any())
            {
                return Result.Failure<string, IErrorBuilder>(ErrorBuilderList.Combine(errors));
            }
            var callStringBuilder = new StringBuilder();

            callStringBuilder.Append($"{resultVariableName} = {Function.FunctionName}(");
            callStringBuilder.AppendJoin(", ", arguments);
            callStringBuilder.Append(")");

            stringBuilder.AppendLine(callStringBuilder.ToString());

            return resultVariableName;
        }
    }
}