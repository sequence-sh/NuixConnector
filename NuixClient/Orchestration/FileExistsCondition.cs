using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A condition that requires that a particular file exists
    /// </summary>
    internal class FileExistsCondition : Condition
    {
        /// <summary>
        /// The path of the file to check
        /// </summary>
        [DataMember]
        [Required]
        public string FilePath { get; set; }

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

        public override int GetHashCode()
        {
            return GetDescription().GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is FileExistsCondition fec && FilePath == fec.FilePath;
        }
    }
}