using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal abstract class CoreUnitMethodConverter<TStep> : CoreMethodConverter<TStep>, IRubyFunction<Unit>
        where TStep : ICompoundStep<Unit>
    {
        /// <inheritdoc />
        protected override IRubyBlock Create(IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> dictionary) => new UnitCompoundRubyBlock(this, dictionary);
    }
}