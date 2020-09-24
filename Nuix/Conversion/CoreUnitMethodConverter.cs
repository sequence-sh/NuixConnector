using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal abstract class CoreUnitMethodConverter<TProcess> : CoreMethodConverter<TProcess>, IRubyFunction<Unit>
        where TProcess : ICompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        protected override IRubyBlock Create(IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> dictionary) => new UnitCompoundRubyBlock(this, dictionary);
    }
}