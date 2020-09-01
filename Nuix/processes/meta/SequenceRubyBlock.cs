using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
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

        /// <summary>
        /// The blocks to run.
        /// </summary>
        public IReadOnlyCollection<IUnitRubyBlock> Blocks { get; }


        /// <inheritdoc />
        public IEnumerable<IRubyFunction> FunctionDefinitions => Blocks.SelectMany(x=>x.FunctionDefinitions).Distinct();

        /// <inheritdoc />
        public Result<IReadOnlyCollection<string>, IRunErrors> TryGetArguments(Suffixer suffixer)
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
        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, Suffixer suffixer)
        {
            var lines = new List<string>();

            foreach (var block in Blocks)
            {
                var r = block.GetOptParseLines(hashSetName, suffixer.GetNextChild());

                lines.AddRange(r);
            }

            return lines;
        }

        /// <inheritdoc />
        public Result<string, IRunErrors> GetBlockText(Suffixer suffixer)
        {
            var sb = new StringBuilder();

            foreach (var block in Blocks)
            {
                var r = block.GetBlockText(suffixer.GetNextChild());

                if (r.IsFailure) return r;

                sb.AppendLine(r.Value);
            }

            return sb.ToString();
        }
    }
}