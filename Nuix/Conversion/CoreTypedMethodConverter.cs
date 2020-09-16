using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal abstract class CoreTypedMethodConverter<TProcess, TOutput> : CoreMethodConverter<TProcess>, IRubyFunction<TOutput>
        where TProcess : ICompoundRunnableProcess<TOutput>
    {
        /// <inheritdoc />
        protected override IRubyBlock Create(IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> dictionary) => new TypedCompoundRubyBlock<TOutput>(this, dictionary);
    }
}