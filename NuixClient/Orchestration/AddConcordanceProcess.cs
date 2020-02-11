using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which adds concordance to a case
    /// </summary>
    public class AddConcordanceProcess : IProcess
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public string Name => $"Add concordance from '{FilePath}'";

        /// <summary>
        /// Execute this process
        /// </summary>
        /// <returns></returns>
        public IAsyncEnumerable<ResultLine> Execute()
        {
            var r = OutsideScripting.AddConcordanceToCase(CasePath, FolderName, Description, Custodian, FilePath,
                ConcordanceDateFormat, ConcordanceProfileName);

            return r;
        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The name of the concordance profile to use
        /// </summary>
        [Required]
        [DataMember]
        public string ConcordanceProfileName { get; set; }

        /// <summary>
        /// The concordance date format to use
        /// </summary>
        [Required]
        [DataMember]
        public string ConcordanceDateFormat { get; set; }

        /// <summary>
        /// The path of the concordance file to import
        /// </summary>
        [Required]
        [DataMember]
        public string FilePath { get; set; }

        /// <summary>
        /// The name of the custodian to assign the folder to
        /// </summary>
        [Required]
        [DataMember]
        public string Custodian { get; set; }

        /// <summary>
        /// A description to add to the folder
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
        /// The name of the case to import into
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