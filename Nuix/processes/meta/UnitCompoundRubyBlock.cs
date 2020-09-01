using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
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
        public Result<string, IRunErrors> GetBlockText(Suffixer suffixer)
        {
            var stringBuilder = new StringBuilder();
            var arguments = new List<string>();
            var errors = new List<IRunErrors>();

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
                return RunErrorList.Combine(errors);

            stringBuilder.Append(Function.FunctionName);
            stringBuilder.Append("(");
            stringBuilder.AppendJoin(", ", arguments);
            stringBuilder.Append(")");

            return stringBuilder.ToString();

        }
    }
}