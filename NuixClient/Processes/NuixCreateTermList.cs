using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.processes
{


    /// <summary>
    /// Creates a list of all terms appearing in the case and their frequencies.
    /// </summary>
    public sealed class NuixCreateTermList : RubyScriptWithOutputProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "Create Termlist";

        /// <summary>
        /// The path of the case to examine.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        internal override string ScriptName => "CreateTermlist.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
        }
    }
}