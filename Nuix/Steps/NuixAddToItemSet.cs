using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Parser;
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
    the_case =$utilities.case_factory.open(pathArg)
    itemSet = the_case.findItemSetByName(itemSetNameArg)
    if(itemSet == nil)
        itemSetOptions = {}
        itemSetOptions[:deduplication] = deduplicationArg if deduplicationArg != nil
        itemSetOptions[:description] = descriptionArg if descriptionArg != nil
        itemSetOptions[:deduplicateBy] = deduplicateByArg if deduplicateByArg != nil
        itemSetOptions[:custodianRanking] = custodianRankingArg.split("","") if custodianRankingArg != nil
        itemSet = the_case.createItemSet(itemSetNameArg, itemSetOptions)

        log ""Item Set Created""
    else
        log ""Item Set Found""
    end

    log ""Searching""
    searchOptions = {}
    searchOptions[:order] = orderArg if orderArg != nil
    searchOptions[:limit] = limitArg.to_i if limitArg != nil
    items = the_case.search(searchArg, searchOptions)
    log ""#{items.length} found""
    itemSet.addItems(items)
    log ""items added""
    the_case.close";
    }


    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a particular item set.
    /// Will create a new item set if one doesn't already exist.
    /// </summary>
    public sealed class NuixAddToItemSet : RubyScriptStepBase<Unit>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixAddToItemSetStepFactory.Instance;


        /// <summary>
        /// The path of the case to search.
        /// </summary>
        [Required]
        [StepProperty(1)]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<StringStream> CasePath { get; set; } = null!;

        /// <summary>
        /// The term to search for.
        /// </summary>
        [Required]
        [StepProperty(2)]
        [RubyArgument("searchArg", 2)]
        public IStep<StringStream> SearchTerm { get; set; }= null!;

        /// <summary>
        /// The item set to add results to. Will be created if it doesn't already exist.
        /// </summary>

        [Required]
        [StepProperty(3)]
        [RubyArgument("itemSetNameArg", 3)]
        public IStep<StringStream> ItemSetName { get; set; }= null!;

        /// <summary>
        /// The means of deduplicating items by key and prioritizing originals in a tie-break.
        /// </summary>
        [StepProperty(4)]
        [RubyArgument("deduplicationArg", 4)]
        [DefaultValueExplanation("No deduplication")]
        public IStep<ItemSetDeduplication>? ItemSetDeduplication { get; set; }

        /// <summary>
        /// The description of the item set.
        /// </summary>

        [StepProperty(5)]
        [RubyArgument("descriptionArg", 5)]
        [DefaultValueExplanation("No description")]
        public IStep<StringStream>? ItemSetDescription { get; set; }

        /// <summary>
        /// Whether to deduplicate as a family or individual.
        /// </summary>

        [StepProperty(6)]
        [RubyArgument("deduplicateByArg", 6)]
        [DefaultValueExplanation("Neither")]
        public IStep<DeduplicateBy>? DeduplicateBy { get; set; }

        /// <summary>
        /// A list of custodian names ordered from highest ranked to lowest ranked.
        /// If this parameter is present and the deduplication parameter has not been specified, MD5 Ranked Custodian is assumed.
        /// </summary>

        [StepProperty(7)]
        [RubyArgument("custodianRankingArg", 7)]
        [DefaultValueExplanation("Do not rank custodians")]
        public IStep<List<StringStream>>? CustodianRanking { get; set; }


        /// <summary>
        /// How to order the items to be added to the item set.
        /// </summary>
        [StepProperty(8)]
        [Example("name ASC, item-date DESC")]
        [RubyArgument("orderArg", 8)]
        [DefaultValueExplanation("Do not reorder")]
        public IStep<StringStream>? Order { get; set; }

        /// <summary>
        /// The maximum number of items to add to the item set.
        /// </summary>
        [StepProperty(9)]
        [RubyArgument("limitArg", 9)]
        [DefaultValueExplanation("No limit")]
        public IStep<int>? Limit { get; set; }
    }
}