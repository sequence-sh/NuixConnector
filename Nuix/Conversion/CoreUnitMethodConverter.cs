using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal abstract class CoreUnitMethodConverter<TProcess> : CoreMethodConverter<TProcess>, IRubyFunction<Unit>
        where TProcess : ICompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        protected override IRubyBlock Create(IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> dictionary) => new UnitCompoundRubyBlock(this, dictionary);
    }
}