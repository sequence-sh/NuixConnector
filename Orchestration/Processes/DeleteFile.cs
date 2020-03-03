using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
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
        /// The path to the file to delete
        /// </summary>
        [YamlMember(Order = 2)]
        [Required]
        [DataMember]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string FilePath { get; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                yield return "File Path is empty";
        }

        /// <inheritdoc />
        public override string GetName() => $"Delete {FilePath}";

        /// <inheritdoc />
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously - needs to be async to return IAsyncEnumerable
        public override async IAsyncEnumerable<Result<string>> Execute()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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