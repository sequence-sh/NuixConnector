using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes;
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
        /// </summary>
        [ExampleValue("MyMetadataProfile")]
        [DefaultValueExplanation("Use the Default profile.")]
        [YamlMember(Order = 3)]
        public string? MetadataProfileName { get; set; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The name of the production set to export.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        public string ProductionSetName { get; set; }

        /// <summary>
        /// Where to export the Concordance to.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        public string ExportPath { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        
        [Required]
        [YamlMember(Order = 6)]
        [ExampleValue("C:/Cases/MyCase")]
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
