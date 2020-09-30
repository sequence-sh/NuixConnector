using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Removes particular items from a Nuix production set.
    /// </summary>
    public sealed class NuixRemoveFromProductionSetStepFactory : RubyScriptStepFactory<NuixRemoveFromProductionSet, Unit>
    {
        private NuixRemoveFromProductionSetStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixRemoveFromProductionSet, Unit> Instance { get; } = new NuixRemoveFromProductionSetStepFactory();


        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(4, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };

        /// <inheritdoc />
        public override string FunctionName => "RemoveFromProductionSet";

        /// <inheritdoc />
        public override string RubyFunctionText => @"
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
    }


    /// <summary>
    /// Removes particular items from a Nuix production set.
    /// </summary>
    public sealed class NuixRemoveFromProductionSet : RubyScriptStepUnit
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
            NuixRemoveFromProductionSetStepFactory.Instance;


        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The search term to use for choosing which items to remove.
        /// </summary>
        [StepProperty]
        [DefaultValueExplanation("All items will be removed.")]
        [Example("Tag:sushi")]
        [RubyArgument("searchArg", 2)]
        public IStep<string>? SearchTerm { get; set; }

        /// <summary>
        /// The production set to remove results from.
        /// </summary>
        [Required]
        [StepProperty]
        [RubyArgument("productionSetNameArg", 3)]

        public IStep<string> ProductionSetName { get; set; }= null!;

    }
}