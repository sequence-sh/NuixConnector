using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Removes particular items from a Nuix production set.
    /// </summary>
    public sealed class NuixRemoveFromProductionSetProcessFactory : RubyScriptProcessFactory<NuixRemoveFromProductionSet, Unit>
    {
        private NuixRemoveFromProductionSetProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixRemoveFromProductionSet, Unit> Instance { get; } = new NuixRemoveFromProductionSetProcessFactory();


        /// <inheritdoc />
        public override Version RequiredVersion { get; } = new Version(4, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };
    }


    /// <summary>
    /// Removes particular items from a Nuix production set.
    /// </summary>
    public sealed class NuixRemoveFromProductionSet : RubyScriptProcess
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory =>
            NuixRemoveFromProductionSetProcessFactory.Instance;

        ///// <inheritdoc />
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public override string GetName() => "Remove items from Production Set";

        /// <summary>
        /// The production set to remove results from.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IRunnableProcess<string> ProductionSetName { get; set; }


        /// <summary>
        /// The search term to use for choosing which items to remove.
        /// </summary>
        [RunnableProcessProperty]
        [DefaultValueExplanation("All items will be removed.")]
        [Example("Tag:sushi")]
        public IRunnableProcess<string>? SearchTerm { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        public IRunnableProcess<string> CasePath { get; set; }

        /// <inheritdoc />
        internal override string ScriptText => @"
    the_case = utilities.case_factory.open(pathArg)

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
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("searchArg", SearchTerm, true);
            yield return ("productionSetNameArg", ProductionSetName, false);
        }
    }
}