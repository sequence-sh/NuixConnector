using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// Adds a file or folder to a Nuix Case
    /// </summary>
    public class AddFileProcess : IProcess
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public string Name => $"Add '{FilePath}'";

        /// <summary>
        /// Execute this process
        /// </summary>
        /// <returns></returns>
        public IAsyncEnumerable<ResultLine> Execute()
        {
            var r = OutsideScripting.AddFileToCase(CasePath, FolderName, Description, Custodian, FilePath);
            return r;
        }
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// The path of the file or folder to add to the case
        /// </summary>
        [Required]
        [DataMember]
        public string FilePath { get; set; }

        /// <summary>
        /// The custodian to assign to the new folder
        /// </summary>
        [Required]
        [DataMember]
        public string Custodian { get; set; }

        /// <summary>
        /// The description of the new folder
        /// </summary>
        [Required]
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// The name of the folder to create
        /// </summary>
        [Required]
        [DataMember]
        public string FolderName { get; set; }

        /// <summary>
        /// The path to the case
        /// </summary>
        [Required]
        [DataMember]
        public string CasePath { get; set; }

        /// <summary>
        /// Conditions under which this process will execute
        /// </summary>
        public IReadOnlyCollection<ICondition> Conditions { get; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}