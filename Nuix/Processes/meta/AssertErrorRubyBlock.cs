﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reductech.EDR.Utilities.Processes;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal sealed class AssertErrorRubyBlock : IUnitRubyBlock
    {
        public readonly ImmutableRubyScriptProcess SubProcess;

        public AssertErrorRubyBlock(ImmutableRubyScriptProcess subProcess)
        {
            SubProcess = subProcess;
        }

        /// <inheritdoc />
        public string BlockName => ProcessNameHelper.GetAssertErrorName(SubProcess.Name);

        /// <inheritdoc />
        public IEnumerable<string> FunctionDefinitions => SubProcess.RubyBlocks.SelectMany(x=>x.FunctionDefinitions).Distinct();

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
        {
            var allArguments = new List<string>();

            foreach (var block in SubProcess.RubyBlocks)
                allArguments.AddRange(block.GetArguments(ref blockNumber));


            return allArguments;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetOptParseLines(ref int blockNumber)
        {
            var allLines = new List<string>();
            
            foreach (var block in SubProcess.RubyBlocks)
                allLines.AddRange(block.GetOptParseLines(ref blockNumber));

            return allLines;
        }

        /// <inheritdoc />
        public string GetBlockText(ref int blockNumber)
        {
            var sb = new StringBuilder();
            sb.AppendLine("begin");

            foreach (var subProcessRubyBlock in SubProcess.RubyBlocks)
                sb.AppendLine(subProcessRubyBlock.GetBlockText(ref blockNumber));

            sb.AppendLine("rescue");
            sb.AppendLine("puts 'Exception Caught, as expected'");
            sb.AppendLine("else");
            sb.AppendLine("puts 'Exception expected but not thrown'");
            sb.AppendLine("exit");
            sb.AppendLine("end");

            return sb.ToString();
        }
    }
}