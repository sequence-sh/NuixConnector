using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reductech.EDR.Utilities.Processes;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal sealed class ConditionalRubyBlock : IUnitRubyBlock
    {
        public ConditionalRubyBlock(ITypedRubyBlock<bool> ifBlock, ImmutableRubyScriptProcess thenProcess, ImmutableRubyScriptProcess elseProcess)
        {
            _ifBlock = ifBlock;
            _thenProcess = thenProcess;
            _elseProcess = elseProcess;
        }

        private readonly ITypedRubyBlock<bool> _ifBlock;
        private readonly ImmutableRubyScriptProcess _thenProcess;
        private readonly ImmutableRubyScriptProcess _elseProcess;

        /// <inheritdoc />
        public string BlockName => ProcessNameHelper.GetConditionalName(_ifBlock.BlockName, _thenProcess.Name, _elseProcess.Name);

        /// <inheritdoc />
        public IEnumerable<string> FunctionDefinitions => _ifBlock.FunctionDefinitions
            .Concat(_thenProcess.RubyBlocks.SelectMany(x=>x.FunctionDefinitions))
            .Concat(_elseProcess.RubyBlocks.SelectMany(x=>x.FunctionDefinitions)).Distinct();

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
        {
            var allArguments = new List<string>();

            allArguments.AddRange(_ifBlock.GetArguments(ref blockNumber));

            foreach (var block in _thenProcess.RubyBlocks.Concat(_elseProcess.RubyBlocks))
                allArguments.AddRange(block.GetArguments(ref blockNumber));


            return allArguments;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetOptParseLines(ref int blockNumber)
        {
            var allLines = new List<string>();

            allLines.AddRange(_ifBlock.GetOptParseLines(ref blockNumber));

            foreach (var block in _thenProcess.RubyBlocks.Concat(_elseProcess.RubyBlocks))
                allLines.AddRange(block.GetOptParseLines(ref blockNumber));

            return allLines;
        }

        /// <inheritdoc />
        public string GetBlockText(ref int blockNumber)
        {
            var sb=  new StringBuilder();

            sb.AppendLine(_ifBlock.GetBlockText(ref blockNumber, out var resultVariableName));

            sb.AppendLine($"if({resultVariableName})");

            foreach (var block in _thenProcess.RubyBlocks)
                sb.AppendLine(block.GetBlockText(ref blockNumber));
            sb.AppendLine("else");
            foreach (var block in _elseProcess.RubyBlocks)
                sb.AppendLine(block.GetBlockText(ref blockNumber));
            sb.AppendLine("end");

            return sb.ToString();
        }
    }
}