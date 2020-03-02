using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.processes
{
    /// <summary>
    /// Creates a report detailing the irregular items in a case.
    /// </summary>
    public sealed class NuixCreateIrregularItemsReport : RubyScriptWithOutputProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "Create Irregular Items report";

        /// <summary>
        /// The path to the folder to create the case in.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        internal override string ScriptName => "IrregularItems.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
        }
    }
}