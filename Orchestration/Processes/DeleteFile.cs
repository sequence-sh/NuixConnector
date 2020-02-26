using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Orchestration.Processes
{
    /// <summary>
    /// Delete a file or a directory
    /// </summary>
    public class DeleteFile : Process
    {
        /// <summary>
        /// 
        /// </summary>
        [YamlMember(Order = 2)]
        [Required]
        public string FilePath { get; }

        public override IEnumerable<string> GetArgumentErrors()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                yield return "File Path is empty";
        }

        public override string GetName() => $"Delete {FilePath}";

        public async override IAsyncEnumerable<Result<string>> Execute()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                yield return Result.Success("File deleted");
            }
            else
            {
                yield return Result.Success("File did not exist");
            }
        }
    }
}