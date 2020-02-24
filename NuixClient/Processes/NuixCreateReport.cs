using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.Processes
{
    /// <summary>
    /// Creates a report for a Nuix case.
    /// </summary>
    internal class NuixCreateReport : RubyScriptWithOutputProcess
    {
        public override string GetName() => "Create Report";

        /// <summary>
        /// The path to the folder to create the case in
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        internal override string ScriptName => "CreateReport.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
        }
    }
}