using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// A ruby block that runs several ruby blocks in sequence.
    /// </summary>
    public sealed class SequenceRubyBlock : IUnitRubyBlock
    {
        /// <summary>
        /// Creates a new SequenceRubyBlock.
        /// </summary>
        public SequenceRubyBlock(IReadOnlyCollection<IUnitRubyBlock> blocks) => Blocks = blocks;

        /// <inheritdoc />
        public string Name => string.Join("; ", Blocks.Select(x => x.ToString()));

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <summary>
        /// The blocks to run.
        /// </summary>
        public IReadOnlyCollection<IUnitRubyBlock> Blocks { get; }


        /// <inheritdoc />
        public IEnumerable<IRubyFunction> FunctionDefinitions => Blocks.SelectMany(x=>x.FunctionDefinitions).Distinct();

        /// <inheritdoc />
        public Result<IReadOnlyCollection<string>, IErrorBuilder> TryGetArguments(Suffixer suffixer)
        {
            var arguments = new List<string>();

            foreach (var unitRubyBlock in Blocks)
            {
                var r = unitRubyBlock.TryGetArguments(suffixer.GetNextChild());
                if (r.IsFailure) return r;

                arguments.AddRange(r.Value);
            }

            return arguments;
        }

        /// <inheritdoc />
        public void WriteOptParseLines(string hashSetName, IIndentationStringBuilder sb, Suffixer suffixer)
        {
            foreach (var block in Blocks)
                block.WriteOptParseLines(hashSetName, sb, suffixer.GetNextChild());

        }

        /// <inheritdoc />
        public Result<Unit, IErrorBuilder> TryWriteBlockLines(Suffixer suffixer, IIndentationStringBuilder stringBuilder)
        {
            foreach (var block in Blocks)
            {
                var r = block.TryWriteBlockLines(suffixer.GetNextChild(), stringBuilder);

                if (r.IsFailure) return r;
            }

            return Unit.Default;
        }

    }
}
