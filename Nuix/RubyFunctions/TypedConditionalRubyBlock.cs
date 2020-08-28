//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Reductech.EDR.Connectors.Nuix.processes.meta
//{
//    internal sealed class TypedConditionalRubyBlock<T> : ITypedRubyBlock<T>
//    {
//        public TypedConditionalRubyBlock(ITypedRubyBlock<bool> ifBlock, RubyScriptProcessTyped<T> thenProcess, RubyScriptProcessTyped<T> elseProcess)
//        {
//            _ifBlock = ifBlock;
//            _thenProcess = thenProcess;
//            _elseProcess = elseProcess;
//        }

//        private readonly ITypedRubyBlock<bool> _ifBlock;
//        private readonly RubyScriptProcessTyped<T> _thenProcess;
//        private readonly RubyScriptProcessTyped<T> _elseProcess;

//        /// <inheritdoc />
//        public string BlockName => "Conditional";
//            //ProcessNameHelper.GetConditionalName(_ifBlock.BlockName, _thenProcess.Name, _elseProcess.Name);

//        /// <inheritdoc />
//        public Version RequiredNuixVersion =>
//            new []{_ifBlock.RequiredNuixVersion, _thenProcess.RubyBlock.RequiredNuixVersion, _elseProcess.RubyBlock.RequiredNuixVersion}
//                .OrderByDescending(x => x).FirstOrDefault()?? NuixVersionHelper.DefaultRequiredVersion;


//        /// <inheritdoc />
//        public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures =>
//            _ifBlock.RequiredNuixFeatures.Concat(_thenProcess.RubyBlock.RequiredNuixFeatures)
//                .Concat(_elseProcess.RubyBlock.RequiredNuixFeatures).Distinct().ToList();


//        /// <inheritdoc />
//        public IEnumerable<string> FunctionDefinitions => _ifBlock.FunctionDefinitions
//            .Concat(_thenProcess.RubyBlock.FunctionDefinitions.Concat(_elseProcess.RubyBlock.FunctionDefinitions));

//        /// <inheritdoc />
//        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
//        {
//            var allArguments = new List<string>();

//            allArguments.AddRange(_ifBlock.GetArguments(ref blockNumber));

//            allArguments.AddRange(_thenProcess.RubyBlock.GetArguments(ref blockNumber));
//            allArguments.AddRange(_elseProcess.RubyBlock.GetArguments(ref blockNumber));


//            return allArguments;
//        }

//        /// <inheritdoc />
//        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, ref int blockNumber)
//        {
//            var allLines = new List<string>();

//            allLines.AddRange(_ifBlock.GetOptParseLines(hashSetName, ref blockNumber));

//            allLines.AddRange(_thenProcess.RubyBlock.GetOptParseLines(hashSetName, ref blockNumber));
//            allLines.AddRange(_elseProcess.RubyBlock.GetOptParseLines(hashSetName, ref blockNumber));

//            return allLines;
//        }

//        /// <inheritdoc />
//        public string GetBlockText(ref int blockNumber, out string resultVariableName)
//        {
//            var sb=  new StringBuilder();

//            sb.AppendLine(_ifBlock.GetBlockText(ref blockNumber, out var ifResultVariableName));

//            resultVariableName = "conditionalResult" + blockNumber;

//            sb.AppendLine($"if({ifResultVariableName})");
//            sb.AppendLine(_thenProcess.RubyBlock.GetBlockText(ref blockNumber, out var thenResultVariableName));
//            sb.AppendLine($"{resultVariableName} = {thenResultVariableName}");
//            sb.AppendLine("else");
//            sb.AppendLine(_elseProcess.RubyBlock.GetBlockText(ref blockNumber, out var elseResultVariableName));
//            sb.AppendLine($"{resultVariableName} = {elseResultVariableName}");
//            sb.AppendLine("end");

//            return sb.ToString();
//        }
//    }
//}