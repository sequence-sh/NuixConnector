using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class ToStreamConverter : CoreTypedMethodConverter<ToStream, Stream>
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IStep argumentProcess)> GetArgumentBlocks(ToStream process)
        {
            yield return (TextArgument, process.Text);
        }

        /// <inheritdoc />
        public override string FunctionName => "ToStream";

        /// <inheritdoc />
        public override string FunctionText => $"{TextArgument}";

        public static readonly RubyFunctionParameter TextArgument = new RubyFunctionParameter("textArg", nameof(ToStream.Text), false, null);

        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } = new[] {TextArgument};
    }
}