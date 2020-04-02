using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Utilities.Processes;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal sealed class TypedConditionalRubyBlock<T> : ITypedRubyBlock<T>
    {
        public TypedConditionalRubyBlock(ITypedRubyBlock<bool> ifBlock, ImmutableRubyScriptProcessTyped<T> thenProcess, ImmutableRubyScriptProcessTyped<T> elseProcess)
        {
            _ifBlock = ifBlock;
            _thenProcess = thenProcess;
            _elseProcess = elseProcess;
        }

        private readonly ITypedRubyBlock<bool> _ifBlock;
        private readonly ImmutableRubyScriptProcessTyped<T> _thenProcess;
        private readonly ImmutableRubyScriptProcessTyped<T> _elseProcess;

        /// <inheritdoc />
        public string BlockName => ProcessNameHelper.GetConditionalName(_ifBlock.BlockName, _thenProcess.Name, _elseProcess.Name);

        /// <inheritdoc />
        public string GetBlockText(ref int blockNumber, out string resultVariableName)
        {
            throw new System.NotImplementedException();
        }


        /// <inheritdoc />
        public IEnumerable<string> FunctionDefinitions => _ifBlock.FunctionDefinitions
            .Concat(_thenProcess.RubyBlock.FunctionDefinitions.Concat(_elseProcess.RubyBlock.FunctionDefinitions));

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
        {
            var allArguments = new List<string>();

            allArguments.AddRange(_ifBlock.GetArguments(ref blockNumber));

            allArguments.AddRange(_thenProcess.RubyBlock.GetArguments(ref blockNumber));
            allArguments.AddRange(_elseProcess.RubyBlock.GetArguments(ref blockNumber));


            return allArguments;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetOptParseLines(ref int blockNumber)
        {
            var allLines = new List<string>();

            allLines.AddRange(_ifBlock.GetOptParseLines(ref blockNumber));

            allLines.AddRange(_thenProcess.RubyBlock.GetOptParseLines(ref blockNumber));
            allLines.AddRange(_elseProcess.RubyBlock.GetOptParseLines(ref blockNumber));

            return allLines;
        }
    }
}