using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which creates a new case
    /// </summary>
    public class CreateCaseProcess : Process
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
            var r = OutsideScripting.CreateCaseRuby(CasePath, CaseName, Description, Investigator);

            return r;
        }

        /// <summary>
        /// The name of the case to create
        /// </summary>
        [Required]
        [DataMember]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CaseName { get; set; }


        /// <summary>
        /// The path to the folder to create the case in
        /// </summary>
        [Required]
        [DataMember]
        public string CasePath { get; set; }

        /// <summary>
        /// Name of the investigator
        /// </summary>
        [Required]
        [DataMember]
        public string Investigator { get; set; }

        /// <summary>
        /// Description of the case
        /// </summary>
        [Required]
        [DataMember]
        public string Description { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}