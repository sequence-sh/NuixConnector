using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a particular item set.
    /// Will create a new item set if one doesn't already exist.
    /// </summary>
    public sealed class NuixAddToItemSetStepFactory : RubyScriptStepFactory<NuixAddToItemSet, Unit>
    {
        private NuixAddToItemSetStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixAddToItemSet, Unit> Instance { get; } = new NuixAddToItemSetStepFactory();


        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(4, 0);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.ANALYSIS
        };


        /// <inheritdoc />
        public override string FunctionName => "AddToItemSet";

        /// <inheritdoc />
        public override string RubyFunctionText =>
            @"
    the_case = utilities.case_factory.open(pathArg)
    itemSet = the_case.findItemSetByName(itemSetNameArg)
    if(itemSet == nil)
        itemSetOptions = {}
        itemSetOptions[:deduplication] = deduplicationArg if deduplicationArg != nil
        itemSetOptions[:description] = descriptionArg if descriptionArg != nil
        itemSetOptions[:deduplicateBy] = deduplicateByArg if deduplicateByArg != nil
        itemSetOptions[:custodianRanking] = custodianRankingArg.split("","") if custodianRankingArg != nil
        itemSet = the_case.createItemSet(itemSetNameArg, itemSetOptions)

        puts ""Item Set Created""
    else
        puts ""Item Set Found""
    end

    puts ""Searching""
    searchOptions = {}
    searchOptions[:order] = orderArg if orderArg != nil
    searchOptions[:limit] = limitArg.to_i if limitArg != nil
    items = the_case.search(searchArg, searchOptions)
    puts ""#{items.length} found""
    itemSet.addItems(items)
    puts ""items added""
    the_case.close";
    }


    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a particular item set.
    /// Will create a new item set if one doesn't already exist.
    /// </summary>
    public sealed class NuixAddToItemSet : RubyScriptStepUnit
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixAddToItemSetStepFactory.Instance;


        /// <summary>
        /// The path of the case to search.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The term to search for.
        /// </summary>
        [Required]
        [StepProperty]
        [RubyArgument("searchArg", 2)]
        public IStep<string> SearchTerm { get; set; }= null!;

        /// <summary>
        /// The item set to add results to. Will be created if it doesn't already exist.
        /// </summary>

        [Required]
        [StepProperty]
        [RubyArgument("itemSetNameArg", 3)]
        public IStep<string> ItemSetName { get; set; }= null!;

        /// <summary>
        /// The means of deduplicating items by key and prioritizing originals in a tie-break.
        /// </summary>
        [StepProperty]
        [RubyArgument("deduplicationArg", 4)]
        public IStep<ItemSetDeduplication>? ItemSetDeduplication { get; set; }

        /// <summary>
        /// The description of the item set.
        /// </summary>

        [StepProperty]
        [RubyArgument("descriptionArg", 5)]
        public IStep<string>? ItemSetDescription { get; set; }

        /// <summary>
        /// Whether to deduplicate as a family or individual.
        /// </summary>

        [StepProperty]
        [RubyArgument("deduplicateByArg", 6)]
        public IStep<DeduplicateBy>? DeduplicateBy { get; set; }

        /// <summary>
        /// A list of custodian names ordered from highest ranked to lowest ranked.
        /// If this parameter is present and the deduplication parameter has not been specified, MD5 Ranked Custodian is assumed.
        /// </summary>

        [StepProperty]
        [RubyArgument("custodianRankingArg", 7)]
        public IStep<List<string>>? CustodianRanking { get; set; }


        /// <summary>
        /// How to order the items to be added to the item set.
        /// </summary>
        [StepProperty]
        [Example("name ASC, item-date DESC")]
        [RubyArgument("orderArg", 8)]
        public IStep<string>? Order { get; set; }

        /// <summary>
        /// The maximum number of items to add to the item set.
        /// </summary>
        [StepProperty]
        [RubyArgument("limitArg", 9)]
        public IStep<int>? Limit { get; set; }
    }
}