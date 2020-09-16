//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Reductech.EDR.Connectors.Nuix.processes.meta
//{
//    internal sealed class CheckNumberRubyBlock : ITypedRubyBlock<bool>
//    {
//        private readonly ITypedRubyBlock<int> _numberBlock;

//        private readonly int? _min;

//        private readonly int? _max;

//        public CheckNumberRubyBlock(ITypedRubyBlock<int> numberBlock, int? min, int? max)
//        {
//            _numberBlock = numberBlock;
//            _min = min;
//            _max = max;
//        }

//        /// <inheritdoc />
//        public string BlockName => $"CheckNumber({_numberBlock.BlockName})"; // ProcessNameHelper.GetCheckNumberProcessName(_numberBlock.BlockName);

//        /// <inheritdoc />
//        public Version RequiredNuixVersion => _numberBlock.RequiredNuixVersion;

//        /// <inheritdoc />
//        public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures => _numberBlock.RequiredNuixFeatures;

//        /// <inheritdoc />
//        public IEnumerable<string> FunctionDefinitions => new[] { CheckNumberDefinition }.Concat(_numberBlock.FunctionDefinitions);

//        /// <inheritdoc />
//        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
//        {
//            var args = new List<string>();

//            if (_min.HasValue)
//            {
//                args.Add("--min" + blockNumber);
//                args.Add(_min.Value.ToString());
//            }

//            if (_max.HasValue)
//            {
//                args.Add("--max" + blockNumber);
//                args.Add(_max.Value.ToString());
//            }
//            blockNumber++;

//            args.AddRange(_numberBlock.GetArguments(ref blockNumber));

//            return args;
//        }

//        /// <inheritdoc />
//        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, ref int blockNumber)
//        {
//            var minArg = "min" + blockNumber;
//            var maxArg = "max" + blockNumber;

//            var lines = new List<string>()
//            {
//                $"opts.on('--{minArg} [ARG]') do |o| params[:{minArg}] = o end",
//                $"opts.on('--{maxArg} [ARG]')  do |o| params[:{maxArg}] = o end"

//            };
//            blockNumber++;
//            lines.AddRange(_numberBlock.GetOptParseLines(hashSetName, ref blockNumber));

//            return lines;
//        }

//        /// <inheritdoc />
//        public string GetBlockText(ref int blockNumber, out string resultVariableName)
//        {
//            var sb = new StringBuilder();
//            var minArg = "min" + blockNumber;
//            var maxArg = "max" + blockNumber;
//            blockNumber++;

//            sb.AppendLine(_numberBlock.GetBlockText(ref blockNumber, out var rvn));

//            resultVariableName = $"checkNumberResult{blockNumber}";

//            sb.AppendLine($"{resultVariableName} = isNumberInRange?({rvn}, {RubyScriptCompilationHelper.GetArgumentValueString(minArg)}, {RubyScriptCompilationHelper.GetArgumentValueString(maxArg)})");

//            return sb.ToString();
//        }

//        private const string CheckNumberDefinition =
//            @"def isNumberInRange?(n, min, max)
//    if(min == nil && max == nil)
//       puts ""Error: both min and max are nil""
//    end

//    if(min != nil && min.to_i > n)
//        return false
//    end
//    if(max != nil && max.to_i < n)
//        return false
//    end
//    return true
//end
//";
//    }
//}