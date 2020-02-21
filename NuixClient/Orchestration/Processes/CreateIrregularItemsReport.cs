using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration.Processes
{
    /// <summary>
    /// Creates a report detailing the irregular items in a case.
    /// </summary>
    internal class CreateIrregularItemsReport : RubyScriptWithOutputProcess
    {
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