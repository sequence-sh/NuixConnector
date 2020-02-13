using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// Adds a file or folder to a Nuix Case
    /// </summary>
    internal class AddFileProcess : Process
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Add '{FilePath}'";

        /// <summary>
        /// Execute this process
        /// </summary>
        /// <returns></returns>
        public override IAsyncEnumerable<ResultLine> Execute()
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
        [JsonProperty(Order = 3)]
        public string FilePath { get; set; }

        /// <summary>
        /// The custodian to assign to the new folder
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 4)]
        public string Custodian { get; set; }

        /// <summary>
        /// The description of the new folder
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 5)]
        public string Description { get; set; }

        /// <summary>
        /// The name of the folder to create
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 6)]
        public string FolderName { get; set; }

        /// <summary>
        /// The path to the case
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 7)]
        public string CasePath { get; set; }

        public override bool Equals(object? obj)
        {
            var r = obj is AddFileProcess afp && (Conditions ?? Enumerable.Empty<Condition>()).SequenceEqual(afp.Conditions ?? Enumerable.Empty<Condition>())
                                                 && FilePath == afp.FilePath
                                                 && Custodian == afp.Custodian
                                                 && Description == afp.Description
                                                 && FolderName == afp.FolderName
                                                 && CasePath == afp.CasePath;

            return r;
        }

        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}