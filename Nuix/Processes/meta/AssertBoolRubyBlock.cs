using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public string BlockName => "Assert " + Expected;
            //ProcessNameHelper.GetAssertBoolProcessName(SubBlock.BlockName, Expected);

        /// <inheritdoc />
        public Version RequiredNuixVersion => SubBlock.RequiredNuixVersion;

        /// <inheritdoc />
        public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures => SubBlock.RequiredNuixFeatures;

        /// <inheritdoc />
        public IEnumerable<string> FunctionDefinitions =>
            new[] {
                Expected? AssertTrueFunctionText : AssertFalseFunctionText
                }
                .Concat(SubBlock.FunctionDefinitions);

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
        {
            return SubBlock.GetArguments(ref blockNumber);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, ref int blockNumber)
        {
            return SubBlock.GetOptParseLines(hashSetName, ref blockNumber);
        }

        /// <inheritdoc />
        public string GetBlockText(ref int blockNumber)
        {
            var sb = new StringBuilder();

            sb.AppendLine(SubBlock.GetBlockText(ref blockNumber, out var resultVariableName));

            if(Expected)
                sb.AppendLine($"AssertTrue({resultVariableName})");
            else sb.AppendLine($"AssertFalse({resultVariableName})");

            return sb.ToString();
        }

        private const string AssertFalseFunctionText = @"def AssertFalse(b)
    if(b)
        puts 'Expected false but was true'
        exit
    end
end";

        private const string AssertTrueFunctionText = @"def AssertTrue(b)
    if(!b)
        puts 'Expected true, but was false'
        exit
    end
end";
    }
}