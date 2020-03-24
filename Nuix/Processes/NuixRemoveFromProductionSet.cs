using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Removes particular items from a Nuix production set.
    /// </summary>
    public sealed class NuixRemoveFromProductionSet : RubyScriptProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "Remove items from Production Set";

        /// <summary>
        /// The production set to remove results from.
        /// </summary>
        
        [Required]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ProductionSetName { get; set; }


        /// <summary>
        /// The search term to use for choosing which items to remove.
        /// </summary>
        
        [YamlMember(Order = 4)]
        [DefaultValueExplanation("All items will be removed.")]
        [ExampleValue("Tag:sushi")]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        
        [Required]
        [YamlMember(Order = 5)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }
        

        /// <inheritdoc />
        internal override string ScriptText => @"   the_case = utilities.case_factory.open(pathArg)

    puts ""Searching""

        productionSet = the_case.findProductionSetByName(productionSetNameArg)

        if(productionSet == nil)
            puts ""Production Set Not Found""
        else
            puts ""Production Set Found""

            if searchArg != nil
                items = the_case.searchUnsorted(searchArg)
                productionSetItems = productionSet.getItems();
                itemsToRemove = items.to_a & productionSetItems
                productionSet.removeItems(itemsToRemove)
                puts ""#{itemsToRemove.length} removed""

            else
                previousTotal = getItems().length

                productionSet.removeAllItems()
                puts ""All items (#{previousTotal}) removed""
            end

            

        end

    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "RemoveFromProductionSet";

        /// <inheritdoc />
        internal override IEnumerable<(string arg, string? val, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("productionSetNameArg", SearchTerm, true);
            yield return ("searchArg", ProductionSetName, false);
        }
    }
}