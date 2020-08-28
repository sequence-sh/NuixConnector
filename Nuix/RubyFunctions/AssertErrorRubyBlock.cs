//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Reductech.EDR.Connectors.Nuix.processes.meta
//{
//    internal sealed class AssertErrorRubyBlock : IUnitRubyBlock
//    {
//        public readonly RubyScriptProcessUnit SubProcess;

//        public AssertErrorRubyBlock(RubyScriptProcessUnit subProcess)
//        {
//            SubProcess = subProcess;
//        }

//        /// <inheritdoc />
//        public string BlockName => $"Assert Error({SubProcess.Name})";

//        /// <inheritdoc />
//        public Version RequiredNuixVersion =>
//            SubProcess.RubyBlocks.Select(x =>
//                x.RequiredNuixVersion).OrderByDescending(x => x).FirstOrDefault()?? NuixVersionHelper.DefaultRequiredVersion;

//        /// <inheritdoc />
//        public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures => SubProcess.RubyBlocks.SelectMany(x=>x.RequiredNuixFeatures).Distinct().ToList();

//        /// <inheritdoc />
//        public IEnumerable<string> FunctionDefinitions => SubProcess.RubyBlocks.SelectMany(x=>x.FunctionDefinitions).Distinct();

//        /// <inheritdoc />
//        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
//        {
//            var allArguments = new List<string>();

//            foreach (var block in SubProcess.RubyBlocks)
//                allArguments.AddRange(block.GetArguments(ref blockNumber));


//            return allArguments;
//        }

//        /// <inheritdoc />
//        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, ref int blockNumber)
//        {
//            var allLines = new List<string>();

//            foreach (var block in SubProcess.RubyBlocks)
//                allLines.AddRange(block.GetOptParseLines(hashSetName, ref blockNumber));

//            return allLines;
//        }

//        /// <inheritdoc />
//        public string GetBlockText(ref int blockNumber)
//        {
//            var sb = new StringBuilder();
//            sb.AppendLine("begin");

//            foreach (var subProcessRubyBlock in SubProcess.RubyBlocks)
//                sb.AppendLine(subProcessRubyBlock.GetBlockText(ref blockNumber));

//            sb.AppendLine("rescue");
//            sb.AppendLine("puts 'Exception Caught, as expected'");
//            sb.AppendLine("else");
//            sb.AppendLine("puts 'Exception expected but not thrown'");
//            sb.AppendLine("exit");
//            sb.AppendLine("end");

//            return sb.ToString();
//        }
//    }
//}