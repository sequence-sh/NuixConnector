using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which adds concordance to a case
    /// </summary>
    internal class AddConcordanceProcess : RubyScriptProcess
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName()
        {
            return $"Add concordance from '{FilePath}'";
        }


#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The name of the concordance profile to use
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
        public string ConcordanceProfileName { get; set; }

        /// <summary>
        /// The concordance date format to use
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 4)]
        public string ConcordanceDateFormat { get; set; }

        /// <summary>
        /// The path of the concordance file to import
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
        public string FilePath { get; set; }

        /// <summary>
        /// The name of the custodian to assign the folder to
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 6)]
        public string Custodian { get; set; }

        /// <summary>
        /// A description to add to the folder
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 7)]
        public string Description { get; set; }

        /// <summary>
        /// The name of the folder to create
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 8)]
        public string FolderName { get; set; }

        /// <summary>
        /// The name of the case to import into
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 9)]
        public string CasePath { get; set; }


#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        internal override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        internal override string ScriptName => "AddConcordanceToCase.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", FolderName);
            yield return ("-d", Description);
            yield return ("-c", Custodian);
            yield return ("-f", FilePath);
            yield return ("-z", ConcordanceDateFormat);
            yield return ("-t", ConcordanceProfileName);
        }
    }
}