using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Exports Concordance for a particular production set.
    /// </summary>
    public sealed class NuixExportConcordance : RubyScriptProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => $"Export {ProductionSetName}";


        /// <summary>
        /// The name of the metadata profile to use.
        /// Will use the Default profile if this is null
        /// </summary>
        [DataMember]
        [YamlMember(Order = 3)]
        public string? MetadataProfileName { get; set; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The name of the production set to export.
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 4)]
        public string ProductionSetName { get; set; }

        /// <summary>
        /// Where to export the Concordance to.
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 5)]
        public string ExportPath { get; set; }

        /// <summary>
        /// The path of the case to export Concordance from.
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 6)]
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        internal override string ScriptName => "ExportConcordance.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-x", ExportPath);
            yield return ("-n", ProductionSetName);
            if(MetadataProfileName != null)
                yield return ("-m", MetadataProfileName);
        }
    }
}
