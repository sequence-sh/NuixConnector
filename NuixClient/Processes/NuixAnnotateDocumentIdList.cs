using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using Orchestration;
using YamlDotNet.Serialization;

namespace NuixClient.Processes
{
    /// <summary>
    /// Generates print previews for items in the production set
    /// </summary>
    internal class GeneratePrintPreviews : RubyScriptProcess
    {
        /// <summary>
        /// The production set to generate print previews for
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

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        public override string GetName() => "Generate print previews";

        internal override string ScriptName => "GeneratePrintPreviews.rb";

        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", ProductionSetName);
        }
    }

    /// <summary>
    /// Renumbers the items in the production set.
    /// </summary>
    internal class NuixReorderProductionSet : RubyScriptProcess
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Renumbers the items in the production set.";


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
        /// Selects the method of sorting items during the renumber
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 5)]
        public ProductionSetSortOrder SortOrder { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        internal override string ScriptName => "RenumberProductionSet.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", ProductionSetName);
            yield return ("-s", SortOrder.GetDescription());
        }
    }


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
        /// Specifies the file path of the document ID list.
        /// </summary>
        [DataMember]
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