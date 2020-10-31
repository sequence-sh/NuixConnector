using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// A ruby block where all the arguments are ruby functions.
    /// </summary>
    public sealed class UnitCompoundRubyBlock : CompoundRubyBlock, IUnitRubyBlock
    {
        /// <inheritdoc />
        public UnitCompoundRubyBlock(IRubyFunction<Unit> unitFunction, IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> arguments)
            : base(arguments) => UnitFunction = unitFunction;

        /// <summary>
        /// The final function to run.
        /// </summary>
        public IRubyFunction<Unit> UnitFunction { get; }

        /// <inheritdoc />
        public override IRubyFunction Function => UnitFunction;


        /// <inheritdoc />
        public Result<Unit, IErrorBuilder> TryWriteBlockLines(Suffixer suffixer, IIndentationStringBuilder stringBuilder)
        {
            var arguments = new List<string>();
            var errors = new List<IErrorBuilder>();

            if (Function.RequireUtilities)
                arguments.Add(RubyScriptCompilationHelper.UtilitiesParameterName);

            foreach (var rubyFunctionArgument in Function.Arguments)
            {
                var childSuffixer = suffixer.GetNextChild();
                if (Arguments.TryGetValue(rubyFunctionArgument, out var block))
                {
                    var childResult = block.TryWriteBlockLines(childSuffixer, stringBuilder);

                    if (childResult.IsSuccess)
                        arguments.Add(childResult.Value);
                    else
                        errors.Add(childResult.Error);
                }
                else if (!rubyFunctionArgument.IsOptional)
                    errors.Add(ErrorHelper.MissingParameterError(rubyFunctionArgument.PropertyName,
                        Function.FunctionName));
            }

            if (errors.Any())
                return Result.Failure<Unit, IErrorBuilder>( ErrorBuilderList.Combine(errors));

            var callStringBuilder = new StringBuilder();

            callStringBuilder.Append(Function.FunctionName);
            callStringBuilder.Append("(");
            callStringBuilder.AppendJoin(", ", arguments);
            callStringBuilder.Append(")");

            stringBuilder.AppendLine(callStringBuilder.ToString());

            return Unit.Default;
        }
    }
}