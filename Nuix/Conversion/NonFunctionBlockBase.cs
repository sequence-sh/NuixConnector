using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal.Errors;

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
        public Result<IReadOnlyCollection<string>, IErrorBuilder> TryGetArguments(Suffixer suffixer)
        {
            var arguments = new List<string>();
            var errors = new List<IErrorBuilder>();

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
                return Result.Failure<IReadOnlyCollection<string>, IErrorBuilder>(ErrorBuilderList.Combine(errors));

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