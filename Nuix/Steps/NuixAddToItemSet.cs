using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps.Helpers;
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
    public override IReadOnlyCollection<IRubyHelper> RequiredHelpers { get; }
        = new List<IRubyHelper> { NuixSearch.Instance };

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

    log ""Adding #{items.length} items to item set '#{itemSetNameArg}'""
    itemSet.addItems(items)
    log('Finished adding items', severity: :debug)
";
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
    /// This parameter only has an effect if Item Set does not exist and is created by
    /// this step.
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("deduplicationArg")]
    [DefaultValueExplanation("No deduplication")]
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
    [DefaultValueExplanation("Neither")]
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

    /// <summary>
    /// Pass additional search options to nuix. For an unsorted search (default)
    /// the only available option is defaultFields. When using <code>SortSearch=true</code>
    /// the options are defaultFields, order, and limit.
    /// Please see the nuix API for <code>Case.search</code>
    /// and <code>Case.searchUnsorted</code> for more details.
    /// </summary>
    [RequiredVersion("Nuix", "7.0")]
    [StepProperty(7)]
    [RubyArgument("searchOptionsArg")]
    [DefaultValueExplanation("No search options provided")]
    public IStep<Entity>? SearchOptions { get; set; }

    /// <summary>
    /// By default the search is not sorted by relevance which
    /// increases performance. Set this to true to sort the
    /// search by relevance.
    /// </summary>
    [RequiredVersion("Nuix", "7.0")]
    [StepProperty(8)]
    [RubyArgument("sortArg")]
    [DefaultValueExplanation("false")]
    public IStep<bool>? SortSearch { get; set; }
}

}
