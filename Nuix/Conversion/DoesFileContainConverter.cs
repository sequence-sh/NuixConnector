using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class DoesFileContainConverter : CoreTypedMethodConverter<DoesFileContain, bool>
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IRunnableProcess argumentProcess)> GetArgumentBlocks(DoesFileContain process)
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

        public static readonly RubyFunctionParameter PathParameter = new RubyFunctionParameter("pathArg", false, null);
        public static readonly RubyFunctionParameter TextParameter = new RubyFunctionParameter("textArg", false, null);


        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } =
            new[] {PathParameter, TextParameter};
    }
}