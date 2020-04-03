using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reductech.EDR.Utilities.Processes;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal sealed class AssertBoolRubyBlock : IUnitRubyBlock
    {
        public readonly bool Expected;
        public readonly ITypedRubyBlock<bool> SubBlock;

        public AssertBoolRubyBlock(ITypedRubyBlock<bool> subBlock, bool expected)
        {
            SubBlock = subBlock;
            Expected = expected;
        }


        /// <inheritdoc />
        public string BlockName => ProcessNameHelper.GetAssertBoolProcessName(SubBlock.BlockName, Expected);

        /// <inheritdoc />
        public IEnumerable<string> FunctionDefinitions =>
            new[] {AssertFunctionText}.Concat(SubBlock.FunctionDefinitions);

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
        {
            return SubBlock.GetArguments(ref blockNumber);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetOptParseLines(ref int blockNumber)
        {
            return SubBlock.GetOptParseLines(ref blockNumber);
        }

        /// <inheritdoc />
        public string GetBlockText(ref int blockNumber)
        {
            var sb = new StringBuilder();

            sb.AppendLine(SubBlock.GetBlockText(ref blockNumber, out var resultVariableName));

            sb.AppendLine($"Assert({resultVariableName})");

            return sb.ToString();
        }

        private const string AssertFunctionText = @"def Assert(b)
if(!b)
    puts 'Condition was not met'
    exit
end";
    }
}