using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class NotConverter : CoreTypedMethodConverter<Not, bool>
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IRunnableProcess argumentProcess)> GetArgumentBlocks(Not process)
        {
            yield return (BooleanParameter, process.Boolean);
        }

        /// <inheritdoc />
        public override string FunctionName => "Negate";

        private static readonly RubyFunctionParameter BooleanParameter
            = new RubyFunctionParameter("booleanArg", false, null);

        /// <inheritdoc />
        public override string FunctionText { get; } = $"   return ! {BooleanParameter.ParameterName}";

        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } = new[] {BooleanParameter};
    }
}