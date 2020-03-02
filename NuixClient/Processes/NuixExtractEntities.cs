using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.processes
{
    /// <summary>
    /// Extract Entities from a Nuix Case
    /// </summary>
    public sealed class NuixExtractEntities : RubyScriptWithOutputProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "Extract Entities";

        /// <summary>
        /// The path to the folder to create the case in
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        internal override string ScriptName => "ExtractEntities.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
        }
    }
}