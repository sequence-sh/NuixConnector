using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class WriteFileConverter : CoreTypedMethodConverter<WriteFile, Unit>
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IRunnableProcess argumentProcess)> GetArgumentBlocks(WriteFile process)
        {
            yield return (FolderParameter, process.Folder);
            yield return (FileNameParameter, process.FileName);
            yield return (TextParameter, process.Text);
        }

        /// <inheritdoc />
        public override string FunctionName => "WriteFile";

        /// <inheritdoc />
        public override string FunctionText { get; } = $"File.write( File.join({FolderParameter.ParameterName}, {FileNameParameter.ParameterName}), {TextParameter.ParameterName})";


        public static readonly RubyFunctionParameter FolderParameter = new RubyFunctionParameter("folderArg", false);
        public static readonly RubyFunctionParameter FileNameParameter = new RubyFunctionParameter("fileNameArg", false);
        public static readonly RubyFunctionParameter TextParameter = new RubyFunctionParameter("textArg", false);

        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } = new[] {FolderParameter, FileNameParameter, TextParameter};
    }
}