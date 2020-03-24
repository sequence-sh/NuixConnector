using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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


        /// <inheritdoc />
        internal override string ScriptText => @"   the_case = utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

        if(productionSet == nil)        
            puts ""Production Set Not Found""
        else            
            puts ""Production Set Found""

            options = 
            {
                sourceProductionSetsInData: pathArg == ""true"",
                dataPath: dataPathArg
            }

            failedItemsCount = productionSet.importDocumentIds(options)

            if failedItemsCount == 0
                puts ""All document ids imported successfully""
            else
                puts ""#{failedItemsCount} items failed to import""

        end 

    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "ImportDocumentIds";

        /// <inheritdoc />
        internal override IEnumerable<(string arg, string? val, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("sourceProductionSetsInDataArg", AreSourceProductionSetsInData.ToString().ToLower(), false);
            yield return ("productionSetNameArg", ProductionSetName, false);
            yield return ("dataPathArg", DataPath, false);
        }
    }
}