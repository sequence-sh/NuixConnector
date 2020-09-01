//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Reductech.EDR.Connectors.Nuix.processes.meta
//{
//    internal sealed class WriteFileRubyBlock : IUnitRubyBlock
//    {
//        public readonly string FilePath;
//        public readonly ITypedRubyBlock<string> SubBlock;

//        public WriteFileRubyBlock(string filePath, ITypedRubyBlock<string> subBlock)
//        {
//            FilePath = filePath;
//            SubBlock = subBlock;
//        }


//        /// <inheritdoc />
//        public string BlockName => $"Write File '{SubBlock.BlockName}'";

//        /// <inheritdoc />
//        public Version RequiredNuixVersion => SubBlock.RequiredNuixVersion;

//        /// <inheritdoc />
//        public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures => SubBlock.RequiredNuixFeatures;

//        /// <inheritdoc />
//        public IEnumerable<string> FunctionDefinitions => SubBlock.FunctionDefinitions;

//        /// <inheritdoc />
//        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
//        {
//            var args = new List<string> { "--" + GetArgName(ref blockNumber), FilePath };
//            args.AddRange(SubBlock.GetArguments(ref blockNumber));
//            return args;
//        }

//        /// <inheritdoc />
//        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, ref int blockNumber)
//        {
//            var argName = GetArgName(ref blockNumber);

//            var lines = new List<string>()
//            {
//                $"opts.on('--{argName} [ARG]') do |o| params[:{argName}] = o end"
//            };
//            lines.AddRange(SubBlock.GetOptParseLines(hashSetName, ref blockNumber));

//            return lines;
//        }

//        /// <inheritdoc />
//        public string GetBlockText(ref int blockNumber)
//        {
//            var sb = new StringBuilder();
//            var pathVariable = RubyScriptCompilationHelper.GetArgumentValueString(GetArgName(ref blockNumber));

//            sb.AppendLine(SubBlock.GetBlockText(ref blockNumber, out var resultVariableName));

//            var line = $"File.write( {pathVariable}, {resultVariableName})";

//            sb.AppendLine(line);

//            return sb.ToString();
//        }

//        private static string GetArgName(ref int blockNumber)
//        {
//            var a = "outputPath" + blockNumber;
//            blockNumber++;
//            return a;
//        }
//    }
//}