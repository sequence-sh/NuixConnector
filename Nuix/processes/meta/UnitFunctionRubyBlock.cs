using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
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
        public override string ToString() => Function.FunctionName;


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
        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, Suffixer suffixer)
        {
            var optParseLines = new List<string>();
            var number = suffixer.GetNext();

            foreach (var argument in Function.Arguments)
            {
                var newParameterName = argument + number;
                var argTerm = argument.IsOptional ? "[ARG]" : "ARG";

                optParseLines.Add($"opts.on('--{newParameterName} {argTerm}') do |o| {hashSetName}[:{newParameterName}] = o end");
            }

            return optParseLines;
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
        public Result<string, IRunErrors> GetBlockText(Suffixer suffixer, out string resultVariableName)
        {
            var number = suffixer.GetNext();

            resultVariableName = Function.FunctionName + number;
            var callStringBuilder = new StringBuilder($"{resultVariableName} = {Function.FunctionName}(");

            var arguments = GetArguments(number);

            callStringBuilder.AppendJoin(", ", arguments);

            callStringBuilder.Append(")");

            return callStringBuilder.ToString();
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
        public Result<string, IRunErrors> GetBlockText(Suffixer suffixer)
        {
            var suffix = suffixer.GetNext();
            var callStringBuilder = new StringBuilder($"{Function.FunctionName}(");

            callStringBuilder.AppendJoin(", ", GetArguments(suffix));

            callStringBuilder.Append(")");

            return callStringBuilder.ToString();
        }
    }



}
//    internal abstract class AbstractBasicRubyBlock : IRubyBlock
//    {
//        protected AbstractBasicRubyBlock(string blockName, string functionText, IReadOnlyCollection<RubyMethodParameter> methodParameters, bool utilitiesParameter)
//        {
//            BlockName = blockName;
//            MethodParameters = methodParameters;
//            FunctionDefinitions = new List<string>{MakeRubyFunction(blockName, functionText, methodParameters, utilitiesParameter)} ;
//        }

//        /// <inheritdoc />
//        public string BlockName { get; }

//        /// <inheritdoc />
//        public int NumberOfArguments => MethodParameters.Count;

//        /// <inheritdoc />
//        public abstract Version RequiredNuixVersion { get; }

//        /// <inheritdoc />
//        public abstract IReadOnlyCollection<NuixFeature> RequiredNuixFeatures { get; }

//        /// <inheritdoc />
//        public IEnumerable<string> FunctionDefinitions { get; }

//        protected readonly IReadOnlyCollection<RubyMethodParameter> MethodParameters;

//        /// <inheritdoc />
//        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
//        {
//            var results = new List<string>();

//            foreach (var (argumentName, argumentValue, _) in MethodParameters)
//            {
//                var newParameterName = argumentName + blockNumber;
//                if (argumentValue == null) continue;

//                results.Add($"--{newParameterName}");
//                results.Add(argumentValue);
//            }

//            blockNumber++;
//            return results;
//        }

//        /// <inheritdoc />
//        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, ref int blockNumber)
//        {
//            var optParseLines = new List<string>();

//            foreach (var (argumentName, _, valueCanBeNull) in MethodParameters)
//            {
//                var newParameterName = argumentName + blockNumber;
//                var argTerm = valueCanBeNull ? "[ARG]" : "ARG";

//                optParseLines.Add($"opts.on('--{newParameterName} {argTerm}') do |o| {hashSetName}[:{newParameterName}] = o end");
//            }

//            blockNumber++;

//            return optParseLines;
//        }

//        public const string UtilitiesParameterName = "utilities";


//        private static string MakeRubyFunction(string methodName,
//            string functionText,
//            IEnumerable<RubyMethodParameter> methodParameters,
//            bool utilitiesParameter)
//        {
//            var methodBuilder = new StringBuilder();
//            var parameters = methodParameters.Select(x => x.ParameterName);
//            if (utilitiesParameter)
//                parameters = parameters.Prepend(UtilitiesParameterName);


//            var methodHeader = $@"def {methodName}({string.Join(",", parameters )})";

//            methodBuilder.AppendLine(methodHeader);
//            methodBuilder.AppendLine(functionText);
//            methodBuilder.AppendLine("end");

//            return methodBuilder.ToString();
//        }

//    }
//}