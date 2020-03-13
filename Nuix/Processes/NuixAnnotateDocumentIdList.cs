using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Annotates a document ID list to add production set names to it.
    /// </summary>
    internal class NuixAnnotateDocumentIdList : RubyScriptProcess
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Annotates a document ID list";


        /// <summary>
        /// The production set to get names from.
        /// </summary>
        
        [Required]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ProductionSetName { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }

        /// <summary>
        /// Specifies the file path of the document ID list.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        public string DataPath { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public override IEnumerable<string> GetArgumentErrors()
        {
            if (!File.Exists(DataPath))
                yield return $"'{DataPath}' does not exist";
        }

        internal override string ScriptName => "AnnotateDocumentIds.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", ProductionSetName);
            yield return ("-d", DataPath);
        }
    }
}