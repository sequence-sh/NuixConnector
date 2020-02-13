﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which adds concordance to a case
    /// </summary>
    internal class AddConcordanceProcess : Process
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName()
        {
            return $"Add concordance from '{FilePath}'";
        }

        /// <summary>
        /// Execute this process
        /// </summary>
        /// <returns></returns>
        public override IAsyncEnumerable<ResultLine> Execute()
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
        [JsonProperty(Order = 3)]
        public string ConcordanceProfileName { get; set; }

        /// <summary>
        /// The concordance date format to use
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 4)]
        public string ConcordanceDateFormat { get; set; }

        /// <summary>
        /// The path of the concordance file to import
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 5)]
        public string FilePath { get; set; }

        /// <summary>
        /// The name of the custodian to assign the folder to
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 6)]
        public string Custodian { get; set; }

        /// <summary>
        /// A description to add to the folder
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 7)]
        public string Description { get; set; }

        /// <summary>
        /// The name of the folder to create
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 8)]
        public string FolderName { get; set; }

        /// <summary>
        /// The name of the case to import into
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 9)]
        public string CasePath { get; set; }

        public override bool Equals(object? obj)
        {
            var r = obj is AddConcordanceProcess acp && (Conditions ?? Enumerable.Empty<Condition>()).SequenceEqual(acp.Conditions ?? Enumerable.Empty<Condition>())
                                                    && ConcordanceProfileName == acp.ConcordanceProfileName
                                                    && ConcordanceDateFormat == acp.ConcordanceDateFormat
                                                    && FilePath == acp.FilePath
                                                    && Custodian == acp.Custodian
                                                    && Description == acp.Description
                                                    && FolderName == acp.FolderName
                                                    && CasePath == acp.CasePath;

            return r;
        }

        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}