using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class AssertErrorConverter : ICoreMethodConverter
    {
        /// <inheritdoc />
        public Result<IRubyBlock> Convert(IStep step)
        {
            if (step is AssertError assertError)
            {
                var r =
                    RubyBlockConversion.TryConvert(assertError.Test, nameof(AssertError.Test))
                        .BindCast<IRubyBlock, IUnitRubyBlock>()
                        .Map(x=> new AssertErrorBlock(x) as IRubyBlock);

                return r;
            }

            return Result.Failure<IRubyBlock>("Could not convert.");
        }

        internal sealed class AssertErrorBlock : NonFunctionBlockBase, IUnitRubyBlock
        {
            /// <inheritdoc />
            public AssertErrorBlock(IUnitRubyBlock testBlock) => TestBlock = testBlock;

            /// <summary>
            /// The block to test for exceptions.
            /// </summary>
            public IUnitRubyBlock TestBlock { get; }


            /// <inheritdoc />
            public override string Name => $"AssertError({TestBlock.Name})";

            /// <inheritdoc />
            public override IEnumerable<IRubyFunction> FunctionDefinitions => TestBlock.FunctionDefinitions;

            /// <inheritdoc />
            public Result<Unit, IErrorBuilder> TryWriteBlockLines(Suffixer suffixer, IIndentationStringBuilder stringBuilder)
            {
                stringBuilder.AppendLine("begin");

                var testBlockResult = TestBlock.TryWriteBlockLines(suffixer, stringBuilder.Indent());
                if (testBlockResult.IsFailure) return testBlockResult;

                stringBuilder.AppendLine("rescue");

                stringBuilder.Indent().AppendLine("#This error was expected");

                stringBuilder.AppendLine("else");

                stringBuilder.Indent().AppendLine("Raise 'Expected an error but none was raised.'");

                stringBuilder.AppendLine("end");

                return Unit.Default;
            }

            /// <inheritdoc />
            protected override IEnumerable<IRubyBlock> OrderedBlocks => new[] {TestBlock};
        }
    }
}