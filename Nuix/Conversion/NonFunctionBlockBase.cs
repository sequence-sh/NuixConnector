using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    /// <summary>
    /// A ruby block that doesn't actually run a function.
    /// </summary>
    public abstract class NonFunctionBlockBase : IRubyBlock
    {
        /// <inheritdoc />
        public override string ToString() => Name;

        /// <summary>
        /// The name of this block.
        /// </summary>
        public abstract string Name { get; }


        /// <inheritdoc />
        public abstract IEnumerable<IRubyFunction> FunctionDefinitions { get; }// = ImmutableList<IRubyFunction>.Empty;

        /// <summary>
        /// The constituent blocks in the order that they will be called.
        /// </summary>
        protected abstract IEnumerable<IRubyBlock> OrderedBlocks { get; }

        /// <inheritdoc />
        public Result<IReadOnlyCollection<string>, IRunErrors> TryGetArguments(Suffixer suffixer)
        {
            var arguments = new List<string>();
            var errors = new List<IRunErrors>();

            foreach (var block in OrderedBlocks)
            {
                var childSuffixer = suffixer.GetNextChild();

                var r = block.TryGetArguments(childSuffixer);

                if(r.IsSuccess)
                    arguments.AddRange(r.Value);
                else
                    errors.Add(r.Error);

            }

            if (errors.Any())
                return RunErrorList.Combine(errors);

            return arguments;
        }

        /// <inheritdoc />
        public void WriteOptParseLines(string hashSetName, IIndentationStringBuilder sb, Suffixer suffixer)
        {
            foreach (var block in OrderedBlocks)
            {
                var childSuffixer = suffixer.GetNextChild();
                block.WriteOptParseLines(hashSetName, sb, childSuffixer);
            }
        }

    }
}