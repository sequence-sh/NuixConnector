using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class DoesFileContainConverter : CoreTypedMethodConverter<DoesFileContain, bool>
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IStep argumentProcess)> GetArgumentBlocks(DoesFileContain process)
        {
            yield return (PathParameter, process.Path);
            yield return (TextParameter, process.Text);
        }

        /// <inheritdoc />
        public override string FunctionName => "DoesFileContain";

        /// <inheritdoc />
        public override string FunctionText { get; } =
$@"
    fileText = File.read({PathParameter.ParameterName})
    return fileText.include? {TextParameter.ParameterName}";

        public static readonly RubyFunctionParameter PathParameter = new RubyFunctionParameter("pathArg", nameof(DoesFileContain.Path), false, null);
        public static readonly RubyFunctionParameter TextParameter = new RubyFunctionParameter("textArg",nameof(DoesFileContain.Text), false, null);


        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } =
            new[] {PathParameter, TextParameter};
    }
}