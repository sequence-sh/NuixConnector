using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which exports concordance for a particular production set
    /// </summary>
    internal class ExportConcordanceProcess : Process
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Export {ProductionSetName}";

        /// <summary>
        /// Execute this process
        /// </summary>
        /// <returns></returns>
        public override IAsyncEnumerable<ResultLine> Execute()
        {
            var r = OutsideScripting.ExportProductionSetConcordance(CasePath, ExportPath, ProductionSetName,
                MetadataProfileName?? "Default");

            return r;
        }

        /// <summary>
        /// The name of the metadata profile to use - "Default" by default
        /// </summary>
        [DataMember]
        [JsonProperty(Order = 3)]
        public string? MetadataProfileName { get; set; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The name of the production set to export
        /// </summary>
        [DataMember]
        [Required]
        [JsonProperty(Order = 4)]
        public string ProductionSetName { get; set; }


        /// <summary>
        /// Where to export the concordance to
        /// </summary>
        [DataMember]
        [Required]
        [JsonProperty(Order = 5)]
        public string ExportPath { get; set; }

        /// <summary>
        /// The path of the case to export
        /// </summary>
        [DataMember]
        [Required]
        [JsonProperty(Order = 6)]
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public override bool Equals(object? obj)
        {
            var r = obj is ExportConcordanceProcess ecp && (Conditions ?? Enumerable.Empty<Condition>()).SequenceEqual(ecp.Conditions ?? Enumerable.Empty<Condition>())
                                                 && MetadataProfileName == ecp.MetadataProfileName
                                                 && ProductionSetName == ecp.ProductionSetName
                                                 && ExportPath == ecp.ExportPath
                                                 && CasePath == ecp.CasePath;

            return r;
        }

        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }
    }
}
