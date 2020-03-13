using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Creates a new case.
    /// </summary>
    internal class NuixCreateCase : RubyScriptProcess
    {
        
        public override string GetName() => "Create Case";

        /// <summary>
        /// The name of the case to create.
        /// </summary>
        [Required]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CaseName { get; set; }

        /// <summary>
        /// The path to the folder to create the case in.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }

        /// <summary>
        /// Name of the investigator.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        public string Investigator { get; set; }

        /// <summary>
        /// Description of the case.
        /// </summary>
        
        [YamlMember(Order = 6)]
        public string? Description { get; set; }


#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        internal override string ScriptName => "CreateCase.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", CaseName);
            if(Description != null)
                yield return ("-d", Description);
            yield return ("-i", Investigator);
        }
    }
}