//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Reductech.EDR.Processes.General;

//namespace Reductech.EDR.Connectors.Nuix.processes.meta
//{
//    internal sealed class ConditionalRubyBlock : IUnitRubyBlock
//    {
//        public ConditionalRubyBlock(ITypedRubyBlock<bool> ifBlock, RubyScriptProcessUnit thenProcess, RubyScriptProcessUnit elseProcess)
//        {
//            _ifBlock = ifBlock;
//            _thenProcess = thenProcess;
//            _elseProcess = elseProcess;
//        }

//        private readonly ITypedRubyBlock<bool> _ifBlock;
//        private readonly RubyScriptProcessUnit _thenProcess;
//        private readonly RubyScriptProcessUnit _elseProcess;

//        /// <inheritdoc />
//        public string BlockName => "Conditional"; //TODO fix
//            //ConditionalProcessFactory.Instance.ProcessNameBuilder. ProcessNameHelper.GetConditionalName(_ifBlock.BlockName, _thenProcess.Name, _elseProcess.Name);

//        /// <inheritdoc />
//        public Version RequiredNuixVersion =>
//            new []{_ifBlock.RequiredNuixVersion}
//                .Concat(_thenProcess.RubyBlocks.Select(x=>x.RequiredNuixVersion))
//                .Concat(_elseProcess.RubyBlocks.Select(x=>x.RequiredNuixVersion))
//                .OrderByDescending(x => x).FirstOrDefault()?? NuixVersionHelper.DefaultRequiredVersion;


//        /// <inheritdoc />
//        public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures =>
//            _ifBlock.RequiredNuixFeatures.Concat(_thenProcess.RubyBlocks.SelectMany(r => r.RequiredNuixFeatures))
//                .Concat(_elseProcess.RubyBlocks.SelectMany(r => r.RequiredNuixFeatures)).Distinct().ToList();

//        /// <inheritdoc />
//        public IEnumerable<string> FunctionDefinitions => _ifBlock.FunctionDefinitions
//            .Concat(_thenProcess.RubyBlocks.SelectMany(x=>x.FunctionDefinitions))
//            .Concat(_elseProcess.RubyBlocks.SelectMany(x=>x.FunctionDefinitions)).Distinct();

//        /// <inheritdoc />
//        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
//        {
//            var allArguments = new List<string>();

//            allArguments.AddRange(_ifBlock.GetArguments(ref blockNumber));

//            foreach (var block in _thenProcess.RubyBlocks.Concat(_elseProcess.RubyBlocks))
//                allArguments.AddRange(block.GetArguments(ref blockNumber));


//            return allArguments;
//        }

//        /// <inheritdoc />
//        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, ref int blockNumber)
//        {
//            var allLines = new List<string>();

//            allLines.AddRange(_ifBlock.GetOptParseLines(hashSetName, ref blockNumber));

//            foreach (var block in _thenProcess.RubyBlocks.Concat(_elseProcess.RubyBlocks))
//                allLines.AddRange(block.GetOptParseLines(hashSetName, ref blockNumber));

//            return allLines;
//        }

//        /// <inheritdoc />
//        public string GetBlockText(ref int blockNumber)
//        {
//            var sb=  new StringBuilder();

//            sb.AppendLine(_ifBlock.GetBlockText(ref blockNumber, out var resultVariableName));

//            sb.AppendLine($"if({resultVariableName})");

//            foreach (var block in _thenProcess.RubyBlocks)
//                sb.AppendLine(block.GetBlockText(ref blockNumber));
//            sb.AppendLine("else");
//            foreach (var block in _elseProcess.RubyBlocks)
//                sb.AppendLine(block.GetBlockText(ref blockNumber));
//            sb.AppendLine("end");

//            return sb.ToString();
//        }
//    }
//}