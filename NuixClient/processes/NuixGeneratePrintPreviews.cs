using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.processes
{
    /// <summary>
    /// Generates print previews for items in the production set
    /// </summary>
    public sealed class NuixGeneratePrintPreviews : RubyScriptProcess
    {

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "Generate print previews";
        /// <summary>
        /// The production set to generate print previews for
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ProductionSetName { get; set; }

        /// <summary>
        /// The path of the case to search
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 4)]
        public string CasePath { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        

        internal override string ScriptName => "GeneratePrintPreviews.rb";

        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", ProductionSetName);
        }
    }
}