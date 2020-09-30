using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal abstract class CoreTypedMethodConverter<TStep, TOutput> : CoreMethodConverter<TStep>, IRubyFunction<TOutput>
        where TStep : ICompoundStep<TOutput>
    {
        /// <inheritdoc />
        protected override IRubyBlock Create(IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> dictionary) => new TypedCompoundRubyBlock<TOutput>(this, dictionary);
    }
}