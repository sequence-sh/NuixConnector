using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Connectors.Nuix.Processes.Meta
{
    /// <summary>
    /// A ruby block that runs a single function
    /// </summary>
    public abstract class FunctionRubyBlock : IRubyBlock
    {
        /// <summary>
        /// Creates a new FunctionRubyBlock
        /// </summary>
        /// <param name="argumentValues"></param>
        protected FunctionRubyBlock(IReadOnlyDictionary<RubyFunctionParameter, string> argumentValues) => ArgumentValues = argumentValues;


        /// <summary>
        /// The function to run.
        /// </summary>
        public abstract IRubyFunction Function { get; }

        /// <inheritdoc />
        public string Name => Function.FunctionName;

        /// <inheritdoc />
        public override string ToString() => Name;


        /// <summary>
        /// The function arguments.
        /// </summary>
        public IReadOnlyDictionary<RubyFunctionParameter, string> ArgumentValues { get; }


        /// <inheritdoc />
        public IEnumerable<IRubyFunction> FunctionDefinitions => new[] { Function };

        /// <inheritdoc />
        public Result<IReadOnlyCollection<string>, IRunErrors> TryGetArguments(Suffixer suffixer)
        {
            var results = new List<string>();
            var num = suffixer.GetNext();

            var errors = new List<IRunErrors>();

            foreach (var argument in Function.Arguments)
            {
                if (ArgumentValues.TryGetValue(argument, out var argumentValue))
                {
                    var parameterName = argument.ParameterName + num;
                    results.Add($"--{parameterName}");
                    results.Add(argumentValue);
                }
                else if (!argument.IsOptional)
                    errors.Add(ErrorHelper.MissingParameterError(argument.ParameterName, Function.FunctionName));
            }

            if (errors.Any())
                return RunErrorList.Combine(errors);

            return results;
        }

        /// <inheritdoc />
        public void WriteOptParseLines(string hashSetName, IIndentationStringBuilder sb, Suffixer suffixer)
        {
            var number = suffixer.GetNext();

            foreach (var argument in Function.Arguments)
            {
                var newParameterName = argument + number;
                var argTerm = argument.IsOptional ? "[ARG]" : "ARG";

                sb.AppendLine($"opts.on('--{newParameterName} {argTerm}') do |o| {hashSetName}[:{newParameterName}] = o end");
            }
        }


        internal IEnumerable<string> GetArguments(string suffix)
        {
            if (Function.RequireUtilities)
                yield return RubyScriptCompilationHelper.UtilitiesParameterName;

            foreach (var argument in Function.Arguments)
            {
                var newParameterName = argument.ParameterName + suffix;
                yield return $"{RubyScriptCompilationHelper.HashSetName}[:{newParameterName}]";
            }
        }
    }

    /// <summary>
    /// A ruby block that runs a single function and returns the result.
    /// </summary>
    public sealed class TypedFunctionRubyBlock<T> : FunctionRubyBlock, ITypedRubyBlock<T>
    {
        /// <summary>
        /// Creates a new TypedFunctionRubyBlock
        /// </summary>
        public TypedFunctionRubyBlock(IRubyFunction<T> function, IReadOnlyDictionary<RubyFunctionParameter, string> argumentValues) : base(argumentValues)
        {
            TypedFunction = function;
        }

        /// <summary>
        /// The function.
        /// </summary>
        public IRubyFunction<T> TypedFunction { get; }

        /// <inheritdoc />
        public override IRubyFunction Function => TypedFunction;


        /// <inheritdoc />
        public Result<string, IRunErrors> TryWriteBlockLines(Suffixer suffixer, IIndentationStringBuilder stringBuilder)
        {
            var number = suffixer.GetNext();
            var resultVariableName = Function.FunctionName + number;
            var callStringBuilder = new StringBuilder($"{resultVariableName} = {Function.FunctionName}(");

            var arguments = GetArguments(number);

            callStringBuilder.AppendJoin(", ", arguments);

            callStringBuilder.Append(")");

            stringBuilder.AppendLine(callStringBuilder.ToString());

            return resultVariableName;
        }
    }
    /// <summary>
    /// A ruby block that runs a single function.
    /// </summary>
    public sealed class UnitFunctionRubyBlock : FunctionRubyBlock, IUnitRubyBlock
    {
        /// <summary>
        /// Creates a new UnitFunctionRubyBlock
        /// </summary>
        public UnitFunctionRubyBlock(IRubyFunction<Unit> function, IReadOnlyDictionary<RubyFunctionParameter, string> argumentValues) : base(argumentValues)
        {
            UnitFunction = function;
        }

        /// <summary>
        /// The function.
        /// </summary>
        public IRubyFunction<Unit> UnitFunction { get; }

        /// <inheritdoc />
        public override IRubyFunction Function => UnitFunction;


        /// <inheritdoc />
        public Result<Unit, IRunErrors> TryWriteBlockLines(Suffixer suffixer, IIndentationStringBuilder stringBuilder)
        {
            var suffix = suffixer.GetNext();
            var callStringBuilder = new StringBuilder($"{Function.FunctionName}(");

            callStringBuilder.AppendJoin(", ", GetArguments(suffix));

            callStringBuilder.Append(")");

            stringBuilder.AppendLine(callStringBuilder.ToString());

            return Unit.Default;
        }
    }



}