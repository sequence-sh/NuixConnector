using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which creates a new case
    /// </summary>
    internal class CreateCaseProcess : Process
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Create Case '{CaseName}'";

        /// <summary>
        /// Execute this process
        /// </summary>
        /// <returns></returns>
        public override IAsyncEnumerable<ResultLine> Execute()
        {
            var r = OutsideScripting.CreateCase(CasePath, CaseName, Description, Investigator);

            return r;
        }

        /// <summary>
        /// The name of the case to create
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CaseName { get; set; }


        /// <summary>
        /// The path to the folder to create the case in
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 4)]
        public string CasePath { get; set; }

        /// <summary>
        /// Name of the investigator
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 5)]
        public string Investigator { get; set; }

        /// <summary>
        /// Description of the case
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 6)]
        public string Description { get; set; }


        public override bool Equals(object? obj)
        {
            var r = obj is CreateCaseProcess ccp && (Conditions ?? Enumerable.Empty<Condition>()).SequenceEqual(ccp.Conditions ?? Enumerable.Empty<Condition>())
                                                     && CaseName == ccp.CaseName
                                                     && Description == ccp.Description
                                                     && Investigator == ccp.Investigator 
                                                     && CasePath == ccp.CasePath;

            return r;
        }

        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}