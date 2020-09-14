using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class AssertTrueConverter : CoreUnitMethodConverter<AssertTrue>
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IRunnableProcess argumentProcess)> GetArgumentBlocks(AssertTrue process)
        {
            yield return (TestParameter, process.Test);
        }

        /// <inheritdoc />
        public override string FunctionName => "AssertTrue";

        /// <inheritdoc />
        public override string FunctionText { get; } = $@"if !{TestParameter.ParameterName}
    raise 'Assertion failed'
    end";

        private static readonly RubyFunctionParameter TestParameter
            = new RubyFunctionParameter("testArg", false);

        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } = new[] {TestParameter};
    }
}