using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class FromStreamConverter : CoreTypedMethodConverter<FromStream, string>
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IStep argumentProcess)> GetArgumentBlocks(FromStream process)
        {
            yield return (StreamArgument, process.Stream);
        }

        /// <inheritdoc />
        public override string FunctionName => "FromStream";

        /// <inheritdoc />
        public override string FunctionText { get; } = $"{StreamArgument}";

        public static readonly RubyFunctionParameter StreamArgument = new RubyFunctionParameter("streamArg", nameof(FromStream.Stream), false, null);

        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } = new[] {StreamArgument};
    }
}