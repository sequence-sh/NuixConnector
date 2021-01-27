using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
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
    public static RubyScriptStepFactory<NuixAddToItemSet, Unit> Instance { get; } =
        new NuixAddToItemSetStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(4, 0);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature> { NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override string FunctionName => "AddToItemSet";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    itemSet = $current_case.findItemSetByName(itemSetNameArg)
    if(itemSet == nil)
        itemSetOptions = {}
        itemSetOptions[:deduplication] = deduplicationArg if deduplicationArg != nil
        itemSetOptions[:description] = descriptionArg if descriptionArg != nil
        itemSetOptions[:deduplicateBy] = deduplicateByArg if deduplicateByArg != nil
        itemSetOptions[:custodianRanking] = custodianRankingArg.split("","") if custodianRankingArg != nil
        itemSet = $current_case.createItemSet(itemSetNameArg, itemSetOptions)

        log ""Item Set Created""
    else
        log ""Item Set Found""
    end

    log ""Searching""
    searchOptions = {}
    searchOptions[:order] = orderArg if orderArg != nil
    searchOptions[:limit] = limitArg.to_i if limitArg != nil
    items = $current_case.search(searchArg, searchOptions)
    log ""#{items.length} found""
    itemSet.addItems(items)
    log ""items added""";
}

/// <summary>
/// Searches a case with a particular search string and adds all items it finds to a particular item set.
/// Will create a new item set if one doesn't already exist.
/// </summary>
[Alias("NuixCreateItemSet")]
public sealed class NuixAddToItemSet : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAddToItemSetStepFactory.Instance;

    /// <summary>
    /// The term to search for.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [RubyArgument("searchArg")]
    [Alias("Search")]
    public IStep<StringStream> SearchTerm { get; set; } = null!;

    /// <summary>
    /// The item set to add results to. Will be created if it doesn't already exist.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("itemSetNameArg")]
    [Alias("Set")]
    public IStep<StringStream> ItemSetName { get; set; } = null!;

    /// <summary>
    /// The means of deduplicating items by key and prioritizing originals in a tie-break.
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("deduplicationArg")]
    [DefaultValueExplanation("No deduplication")]
    [Alias("DeduplicateUsing")]
    public IStep<ItemSetDeduplication>? ItemSetDeduplication { get; set; }

    /// <summary>
    /// The description of the item set.
    /// </summary>
    [StepProperty(4)]
    [RubyArgument("descriptionArg")]
    [DefaultValueExplanation("No description")]
    [Alias("Description")]
    public IStep<StringStream>? ItemSetDescription { get; set; }

    /// <summary>
    /// Whether to deduplicate as a family or individual.
    /// </summary>
    [StepProperty(5)]
    [RubyArgument("deduplicateByArg")]
    [DefaultValueExplanation("Neither")]
    public IStep<DeduplicateBy>? DeduplicateBy { get; set; }

    /// <summary>
    /// A list of custodian names ordered from highest ranked to lowest ranked.
    /// If this parameter is present and the deduplication parameter has not been specified, MD5 Ranked Custodian is assumed.
    /// </summary>
    [StepProperty(6)]
    [RubyArgument("custodianRankingArg")]
    [DefaultValueExplanation("Do not rank custodians")]
    public IStep<Array<StringStream>>? CustodianRanking { get; set; }

    /// <summary>
    /// How to order the items to be added to the item set.
    /// </summary>
    [StepProperty(7)]
    [Example("name ASC, item-date DESC")]
    [RubyArgument("orderArg")]
    [DefaultValueExplanation("Do not reorder")]
    public IStep<StringStream>? Order { get; set; }

    /// <summary>
    /// The maximum number of items to add to the item set.
    /// </summary>
    [StepProperty(8)]
    [RubyArgument("limitArg")]
    [DefaultValueExplanation("No limit")]
    public IStep<int>? Limit { get; set; }
}

}
