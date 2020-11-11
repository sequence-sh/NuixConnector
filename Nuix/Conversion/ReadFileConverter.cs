using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class ReadFileConverter : CoreTypedMethodConverter<ReadFile, Stream>
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IStep argumentProcess)> GetArgumentBlocks(ReadFile process)
        {
            yield return (FolderParameter, process.Folder);
            yield return (FileNameParameter, process.FileName);
        }

        /// <inheritdoc />
        public override string FunctionName => "ReadFile";


        public static readonly RubyFunctionParameter FolderParameter = new RubyFunctionParameter("folderArg", nameof(ReadFile.Folder), false, null);
        public static readonly RubyFunctionParameter FileNameParameter = new RubyFunctionParameter("fileNameArg", nameof(ReadFile.FileName), false, null);

        /// <inheritdoc />
        public override string FunctionText { get; }= "File.read(File.join({FolderParameter.ParameterName}, {FileNameParameter.ParameterName}))";

        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } = new []{FolderParameter, FileNameParameter};
    }
}