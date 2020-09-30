using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class WriteFileConverter : CoreUnitMethodConverter<WriteFile>
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IStep argumentProcess)> GetArgumentBlocks(WriteFile process)
        {
            yield return (FolderParameter, process.Folder);
            yield return (FileNameParameter, process.FileName);
            yield return (TextParameter, process.Text);
        }

        /// <inheritdoc />
        public override string FunctionName => "WriteFile";

        /// <inheritdoc />
        public override string FunctionText { get; } = $"   File.write( File.join({FolderParameter.ParameterName}, {FileNameParameter.ParameterName}), {TextParameter.ParameterName})";


        public static readonly RubyFunctionParameter FolderParameter = new RubyFunctionParameter("folderArg", false, null);
        public static readonly RubyFunctionParameter FileNameParameter = new RubyFunctionParameter("fileNameArg", false, null);
        public static readonly RubyFunctionParameter TextParameter = new RubyFunctionParameter("textArg", false, null);

        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } = new[] {FolderParameter, FileNameParameter, TextParameter};
    }
}