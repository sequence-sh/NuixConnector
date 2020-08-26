using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal abstract class AbstractBasicRubyBlock : IRubyBlock
    {
        protected AbstractBasicRubyBlock(string blockName, string functionText, IReadOnlyCollection<RubyMethodParameter> methodParameters, bool utilitiesParameter)
        {
            BlockName = blockName;
            MethodParameters = methodParameters;
            FunctionDefinitions = new List<string>{MakeRubyFunction(blockName, functionText, methodParameters, utilitiesParameter)} ;
        }

        /// <inheritdoc />
        public string BlockName { get; }

        /// <inheritdoc />
        public abstract Version RequiredNuixVersion { get; }

        /// <inheritdoc />
        public abstract IReadOnlyCollection<NuixFeature> RequiredNuixFeatures { get; }

        /// <inheritdoc />
        public IEnumerable<string> FunctionDefinitions { get; }

        protected readonly IReadOnlyCollection<RubyMethodParameter> MethodParameters;

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
        {
            var results = new List<string>();

            foreach (var (argumentName, argumentValue, _) in MethodParameters)
            {
                var newParameterName = argumentName + blockNumber;
                if (argumentValue == null) continue;

                results.Add($"--{newParameterName}");
                results.Add(argumentValue);
            }

            blockNumber++;
            return results;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, ref int blockNumber)
        {
            var optParseLines = new List<string>();

            foreach (var (argumentName, _, valueCanBeNull) in MethodParameters)
            {
                var newParameterName = argumentName + blockNumber;
                var argTerm = valueCanBeNull ? "[ARG]" : "ARG";

                optParseLines.Add($"opts.on('--{newParameterName} {argTerm}') do |o| {hashSetName}[:{newParameterName}] = o end");
            }

            blockNumber++;

            return optParseLines;
        }

        public const string UtilitiesParameterName = "utilities";


        private static string MakeRubyFunction(string methodName,
            string functionText,
            IEnumerable<RubyMethodParameter> methodParameters,
            bool utilitiesParameter)
        {
            var methodBuilder = new StringBuilder();
            var parameters = methodParameters.Select(x => x.ParameterName);
            if (utilitiesParameter)
                parameters = parameters.Prepend(UtilitiesParameterName);


            var methodHeader = $@"def {methodName}({string.Join(",", parameters )})";

            methodBuilder.AppendLine(methodHeader);
            methodBuilder.AppendLine(functionText);
            methodBuilder.AppendLine("end");

            return methodBuilder.ToString();
        }

    }
}