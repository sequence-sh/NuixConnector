using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class ConditionalConverter : ICoreMethodConverter
    {
        /// <inheritdoc />
        public Result<IRubyBlock> Convert(IRunnableProcess process)
        {
            if (process is Conditional conditional)
            {
                if (conditional.ElseProcess != null)
                {
                    var conversionResult =
                        RubyBlockConversion.TryConvert(conditional.Condition, nameof(conditional.Condition)).BindCast<IRubyBlock, ITypedRubyBlock<bool>>()
                            .Compose(()=> RubyBlockConversion.TryConvert(conditional.ThenProcess, nameof(conditional.ThenProcess)).BindCast<IRubyBlock, IUnitRubyBlock>(),
                                ()=> RubyBlockConversion.TryConvert(conditional.ElseProcess, nameof(conditional.ElseProcess)).BindCast<IRubyBlock, IUnitRubyBlock>())
                            .Map(x=> new IfThenElseBlock(x.Item1, x.Item2, x.Item3) as IRubyBlock);

                    return conversionResult;
                }
                else
                {
                    var conversionResult =
                        RubyBlockConversion.TryConvert(conditional.Condition, nameof(conditional.Condition)).BindCast<IRubyBlock, ITypedRubyBlock<bool>>()
                            .Compose(() => RubyBlockConversion.TryConvert(conditional.ThenProcess, nameof(conditional.ThenProcess)).BindCast<IRubyBlock, IUnitRubyBlock>())
                            .Map(x => new IfThenBlock(x.Item1, x.Item2) as IRubyBlock);

                    return conversionResult;
                }
            }

            return Result.Failure<IRubyBlock>("Could not convert to conditional.");
        }


        internal sealed class IfThenBlock : NonFunctionBlockBase, IUnitRubyBlock
        {
            /// <inheritdoc />
            public IfThenBlock(ITypedRubyBlock<bool> ifBlock, IUnitRubyBlock thenBlock)
            {
                IfBlock = ifBlock;
                ThenBlock = thenBlock;
            }

            /// <summary>
            /// The condition to check
            /// </summary>
            public ITypedRubyBlock<bool> IfBlock { get; }

            /// <summary>
            /// The thing to do if the condition is true.
            /// </summary>
            public IUnitRubyBlock ThenBlock { get; }

            /// <inheritdoc />
            public override string Name => $"If {IfBlock.Name} then {ThenBlock.Name}";

            /// <inheritdoc />
            public override IEnumerable<IRubyFunction> FunctionDefinitions =>
                IfBlock.FunctionDefinitions.Concat(ThenBlock.FunctionDefinitions);

            /// <inheritdoc />
            public Result<Unit, IRunErrors> TryWriteBlockLines(Suffixer suffixer, IIndentationStringBuilder stringBuilder)
            {
                var ifResult = IfBlock.TryWriteBlockLines(suffixer.GetNextChild(), stringBuilder);

                if (ifResult.IsFailure)
                    return ifResult.ConvertFailure<Unit>();


                stringBuilder.AppendLine($"if {ifResult.Value}");

                var thenResult = ThenBlock.TryWriteBlockLines(suffixer, stringBuilder.Indent());

                if (thenResult.IsFailure)
                    return thenResult;

                stringBuilder.AppendLine("end");

                return Unit.Default;
            }

            /// <inheritdoc />
            protected override IEnumerable<IRubyBlock> OrderedBlocks => new IRubyBlock[] {IfBlock, ThenBlock};
        }

        internal sealed class IfThenElseBlock : NonFunctionBlockBase, IUnitRubyBlock
        {
            /// <summary>
            /// Creates a new IfThenElseBlock
            /// </summary>
            public IfThenElseBlock(ITypedRubyBlock<bool> ifBlock, IUnitRubyBlock thenBlock, IUnitRubyBlock elseBlock)
            {
                IfBlock = ifBlock;
                ThenBlock = thenBlock;
                ElseBlock = elseBlock;
            }


            /// <inheritdoc />
            public override string Name => $"if {IfBlock.Name} then {ThenBlock.Name} else {ElseBlock.Name}";

            /// <inheritdoc />
            public Result<Unit, IRunErrors> TryWriteBlockLines(Suffixer suffixer, IIndentationStringBuilder stringBuilder)
            {
                var ifResult = IfBlock.TryWriteBlockLines(suffixer.GetNextChild(), stringBuilder);

                if (ifResult.IsFailure)
                    return ifResult.ConvertFailure<Unit>();

                stringBuilder.AppendLine($"if {ifResult.Value}");

                var thenResult = ThenBlock.TryWriteBlockLines(suffixer.GetNextChild(), stringBuilder.Indent());

                if (thenResult.IsFailure)
                    return thenResult;

                stringBuilder.AppendLine("else");

                var elseResult = ElseBlock.TryWriteBlockLines(suffixer.GetNextChild(), stringBuilder.Indent());

                if (elseResult.IsFailure)
                    return elseResult;

                stringBuilder.AppendLine("end");

                return Unit.Default;
            }

            /// <inheritdoc />
            public override IEnumerable<IRubyFunction> FunctionDefinitions =>
                IfBlock.FunctionDefinitions
                    .Concat(ThenBlock.FunctionDefinitions)
                    .Concat(ElseBlock.FunctionDefinitions);


            /// <summary>
            /// The condition to check
            /// </summary>
            public ITypedRubyBlock<bool> IfBlock { get; }

            /// <summary>
            /// The thing to do if the condition is true.
            /// </summary>
            public IUnitRubyBlock ThenBlock { get; }

            /// <summary>
            /// The thing to do otherwise.
            /// </summary>
            public IUnitRubyBlock ElseBlock { get; }

            /// <inheritdoc />
            protected override IEnumerable<IRubyBlock> OrderedBlocks => new IRubyBlock[] {IfBlock, ThenBlock, ElseBlock};
        }
    }
}