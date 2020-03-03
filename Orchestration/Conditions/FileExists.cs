using System.ComponentModel.DataAnnotations;
using YamlDotNet.Serialization;

namespace Orchestration.Conditions
{
    /// <summary>
    /// A condition that requires that a particular file exists
    /// </summary>
    public class FileExists : Condition
    {
        /// <summary>
        /// The path of the file to check
        /// </summary>
        [YamlMember(Order = 1)]
        [Required]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string FilePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// 
        /// </summary>
        public override string GetDescription() => $"{FilePath} exists";

        /// <summary>
        /// Does the file exist
        /// </summary>
        /// <returns></returns>
        public override bool IsMet()
        {
            return System.IO.File.Exists(FilePath);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return GetDescription().GetHashCode();
        }
        /// <inheritdoc />

        public override bool Equals(object? obj)
        {
            return obj is FileExists fec && FilePath == fec.FilePath;
        }
    }
}