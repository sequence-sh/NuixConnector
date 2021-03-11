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
/// Run a search query in Nuix and add all items found to an item set.
/// Will create a new item set if one doesn't already exist.
/// </summary>
public sealed class NuixAddToItemSetStepFactory : RubySearchStepFactory<NuixAddToItemSet, Unit>
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

    items = search(searchArg, searchOptionsArg, sortArg)
    return unless items.length > 0

    log(""Searching for item set #{itemSetNameArg}"", severity: :debug)
    itemSet = $current_case.findItemSetByName(itemSetNameArg)

    if itemSet.nil?
      log ""Item set '#{itemSetNameArg}' not found. Creating.""
      itemSetOptions = {}
      itemSetOptions[:deduplication] = deduplicationArg unless deduplicationArg.nil?
      itemSetOptions[:description] = descriptionArg unless descriptionArg.nil?
      itemSetOptions[:deduplicateBy] = deduplicateByArg unless deduplicateByArg.nil?
      itemSetOptions[:custodianRanking] = custodianRankingArg.split(',') unless custodianRankingArg.nil?
      log(""Item set options: #{itemSetOptions}"", severity: :trace)
      itemSet = $current_case.create_item_set(itemSetNameArg, itemSetOptions)
      log ""Successfully created item set '#{itemSetNameArg}'""
    else
      log ""Existing item set '#{itemSetNameArg}' found""
    end

    all_items = expand_search(items, searchTypeArg)

    log ""Adding #{all_items.length} items to item set '#{itemSetNameArg}'""
    itemSet.addItems(all_items)
    log('Finished adding items', severity: :debug)
";
}

/// <summary>
/// Run a search query in Nuix and add all items found to an item set.
/// Will create a new item set if one doesn't already exist.
/// </summary>
[Alias("NuixCreateItemSet")]
public sealed class NuixAddToItemSet : RubySearchStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAddToItemSetStepFactory.Instance;

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
    /// This parameter only has an effect if Item Set does not exist and is created by
    /// this step.
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("deduplicationArg")]
    [DefaultValueExplanation("MD5")]
    [Alias("DeduplicateUsing")]
    public IStep<ItemSetDeduplication>? ItemSetDeduplication { get; set; }

    /// <summary>
    /// The description of the item set.
    /// This parameter only has an effect if Item Set does not exist and is created by
    /// this step.
    /// </summary>
    [StepProperty(4)]
    [RubyArgument("descriptionArg")]
    [DefaultValueExplanation("No description")]
    [Alias("Description")]
    public IStep<StringStream>? ItemSetDescription { get; set; }

    /// <summary>
    /// Whether to deduplicate as a family or individual.
    /// This parameter only has an effect if Item Set does not exist and is created by
    /// this step.
    /// </summary>
    [StepProperty(5)]
    [RubyArgument("deduplicateByArg")]
    [DefaultValueExplanation("Individual")]
    public IStep<DeduplicateBy>? DeduplicateBy { get; set; }

    /// <summary>
    /// A list of custodian names ordered from highest ranked to lowest ranked.
    /// If this parameter is present and the deduplication parameter has not been
    /// specified, MD5 Ranked Custodian is assumed.
    /// This parameter only has an effect if Item Set does not exist and is created by
    /// this step.
    /// </summary>
    [StepProperty(6)]
    [RubyArgument("custodianRankingArg")]
    [DefaultValueExplanation("Do not rank custodians")]
    public IStep<Array<StringStream>>? CustodianRanking { get; set; }
}

}
