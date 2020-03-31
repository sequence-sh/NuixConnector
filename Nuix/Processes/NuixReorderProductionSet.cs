using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.enums;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Reorders and renumbers the items in a production set.
    /// </summary>
    public sealed class NuixReorderProductionSet : RubyScriptProcess
    {
        /// <inheritdoc />
        protected override NuixReturnType ReturnType => NuixReturnType.Unit;

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => $"Renumbers the items in the production set.";

        /// <summary>
        /// The production set to reorder.
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
        /// The method of sorting items during the renumbering.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        public ProductionSetSortOrder SortOrder { get; set; } = ProductionSetSortOrder.Position;

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
                sortOrder: sortOrderArg
            }

            resultMap = productionSet.renumber(options)

            puts resultMap

        end 

    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "RenumberProductionSet";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("productionSetNameArg", ProductionSetName, false);
            yield return ("sortOrderArg", SortOrder.GetDescription(), false);
        }
    }
}