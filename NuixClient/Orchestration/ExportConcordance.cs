using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which exports concordance for a particular production set
    /// </summary>
    public class ExportConcordance : IProcess
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public string Name => $"Export {ProductionSetName}";

        /// <summary>
        /// Execute this process
        /// </summary>
        /// <returns></returns>
        public IAsyncEnumerable<ResultLine> Execute()
        {
            var r = OutsideScripting.ExportProductionSetConcordance(CasePath, ExportPath, ProductionSetName,
                MetadataProfileName?? "Default");

            return r;
        }

        /// <summary>
        /// The name of the metadata profile to use - "Default" by default
        /// </summary>
        [DataMember]
        public string? MetadataProfileName { get; set; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The name of the production set to export
        /// </summary>
        [DataMember]
        [Required]
        public string ProductionSetName { get; set; }


        /// <summary>
        /// Where to export the concordance to
        /// </summary>
        [DataMember]
        [Required]
        public string ExportPath { get; set; }

        /// <summary>
        /// The path of the case to export
        /// </summary>
        [DataMember]
        [Required]
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// Conditions under which this process will execute
        /// </summary>
        public IReadOnlyCollection<ICondition>? Conditions { get; }
    }
}
