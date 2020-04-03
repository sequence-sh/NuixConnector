using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reductech.EDR.Utilities.Processes;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal sealed class CheckNumberRubyBlock : ITypedRubyBlock<bool>
    {
        private readonly ITypedRubyBlock<int> _numberBlock;

        private readonly int? _min;

        private readonly int? _max;

        public CheckNumberRubyBlock(ITypedRubyBlock<int> numberBlock, int? min, int? max)
        {
            _numberBlock = numberBlock;
            _min = min;
            _max = max;
        }
        
        /// <inheritdoc />
        public string BlockName => ProcessNameHelper.GetCheckNumberProcessName(_numberBlock.BlockName);

        /// <inheritdoc />
        public IEnumerable<string> FunctionDefinitions => new []{CheckNumberDefinition}.Concat(_numberBlock.FunctionDefinitions);

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
        {
            var args = new List<string>()
            {
                "min" + blockNumber,
                "max" + blockNumber
            };
            blockNumber++;

            args.AddRange(_numberBlock.GetArguments(ref blockNumber));                
                
            return args;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetOptParseLines(ref int blockNumber)
        {
            var minArg = "min" + blockNumber;
            var maxArg = "max" + blockNumber;

            var lines = new List<string>()
            {
                $"opts.on('--{minArg} [ARG]')",
                $"opts.on('--{maxArg} [ARG]')"

            };
            blockNumber++;
            lines.AddRange(_numberBlock.GetOptParseLines(ref blockNumber));

            return lines;
        }

        /// <inheritdoc />
        public string GetBlockText(ref int blockNumber, out string resultVariableName)
        {
            var sb = new StringBuilder();
            var minArg = "min" + blockNumber;
            var maxArg = "max" + blockNumber;
            blockNumber++;

            sb.AppendLine(_numberBlock.GetBlockText(ref blockNumber, out var rvn));

            resultVariableName = $"checkNumberResult{blockNumber}";

            sb.AppendLine($"{resultVariableName} = isNumberInRange?({rvn}, {RubyScriptCompilationHelper.HashSetName}[:{minArg}], {RubyScriptCompilationHelper.HashSetName}[:{maxArg}]) ");

            return sb.ToString();
        }

        private const string CheckNumberDefinition =
            @"def isNumberInRange?(n, min, max)
if(min != nil && min > n)
    return false
end
if(max != nil && max < n)
    return false
end
return true
end
";
    }
}