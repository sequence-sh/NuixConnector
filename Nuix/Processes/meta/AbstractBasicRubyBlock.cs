using System.Collections.Generic;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal abstract class AbstractBasicRubyBlock : IRubyBlock
    {
        protected AbstractBasicRubyBlock(string blockName, string functionText, IReadOnlyCollection<RubyMethodParameter> methodParameters)
        {
            BlockName = blockName;
            MethodParameters = methodParameters;
            FunctionDefinitions = new List<string>{functionText} ;
        }

        /// <inheritdoc />
        public string BlockName { get; }

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

        
    }
}