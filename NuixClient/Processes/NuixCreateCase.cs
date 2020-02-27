using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.Processes
{

    internal class NuixMigrateCase : RubyScriptProcess
    {
        /// <summary>
        /// The path to the case folder
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 2)]
        public string CasePath { get; set; }

        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        public override string GetName() => "Migrate Case";

        internal override string ScriptName => "MigrateCase.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
        }
    }

    /// <summary>
    /// Creates a new case
    /// </summary>
    internal class NuixCreateCase : RubyScriptProcess
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Create Case";

        /// <summary>
        /// The name of the case to create.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CaseName { get; set; }

        /// <summary>
        /// The path to the folder to create the case in.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 4)]
        public string CasePath { get; set; }

        /// <summary>
        /// Name of the investigator.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
        public string Investigator { get; set; }

        /// <summary>
        /// Description of the case.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 6)]
        public string Description { get; set; }


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
            yield return ("-d", Description);
            yield return ("-i", Investigator);
        }
    }
}