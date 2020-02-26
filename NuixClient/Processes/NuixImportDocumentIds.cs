using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.Processes
{
    /// <summary>
    /// Imports the given document IDs into this production set. Only works if this production set has imported numbering.
    /// </summary>
    internal class NuixImportDocumentIds : RubyScriptProcess
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Add document ids to production set.";


        /// <summary>
        /// The production set to add results to. Will be created if it doesn't already exist
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

        /// <summary>
        /// Specifies that the source production set name(s) are contained in the document ID list.
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 5)]
        public bool AreSourceProductionSetsInData { get; set; }

        /// <summary>
        /// Specifies the file path of the document ID list.
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 6)]
        public string DataPath { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public override IEnumerable<string> GetArgumentErrors()
        {
            if (!File.Exists(DataPath))
                yield return $"'{DataPath}' does not exist";
        }

        internal override string ScriptName => "ImportDocumentIds.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-s", AreSourceProductionSetsInData.ToString().ToLower());
            yield return ("-n", ProductionSetName);
            yield return ("-d", DataPath);
        }
    }
}