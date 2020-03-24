using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Imports the given document IDs into this production set. Only works if this production set has imported numbering.
    /// </summary>
    public sealed class NuixImportDocumentIds : RubyScriptProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => $"Add document ids to production set.";

        /// <summary>
        /// The production set to add results to.
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
        /// Specifies that the source production set name(s) are contained in the document ID list.
        /// </summary>

        [Required]
        [YamlMember(Order = 5)]
        public bool AreSourceProductionSetsInData { get; set; } = false;

        /// <summary>
        /// Specifies the file path of the document ID list.
        /// </summary>
        
        [Required]
        [YamlMember(Order = 6)]
        public string DataPath { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


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