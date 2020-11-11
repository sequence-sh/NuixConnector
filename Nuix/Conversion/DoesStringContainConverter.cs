using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{


    internal sealed class DoesStringContainConverter : CoreTypedMethodConverter<DoesStringContain, bool>
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IStep argumentProcess)> GetArgumentBlocks(DoesStringContain process)
        {
            yield return (SubstringParameter, process.Substring);
            yield return (SuperStringParameter, process.Superstring);
            yield return (IgnoreCaseParameter, process.IgnoreCase);
        }

        /// <inheritdoc />
        public override string FunctionName => "Does String Contain";

        /// <inheritdoc />
        public override string FunctionText { get; } = $@"if({IgnoreCaseParameter.ParameterName})
        {SuperStringParameter.ParameterName}.downcase[{SubstringParameter.ParameterName}.downcase]
    else
        {SuperStringParameter.ParameterName}[{SubstringParameter.ParameterName}]
    end";

        public static readonly RubyFunctionParameter SubstringParameter = new RubyFunctionParameter("substringArg", nameof(DoesStringContain.Substring), false, null);
        public static readonly RubyFunctionParameter SuperStringParameter = new RubyFunctionParameter("superstringArg", nameof(DoesStringContain.Superstring), false, null);
        public static readonly RubyFunctionParameter IgnoreCaseParameter = new RubyFunctionParameter("ignoreCaseArg", nameof(DoesStringContain.IgnoreCase), false, null);

        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } = new []{SubstringParameter, SuperStringParameter, IgnoreCaseParameter};
    }
}