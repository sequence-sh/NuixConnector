using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Removes particular items from a Nuix production set.
/// </summary>
public sealed class
    NuixRemoveFromItemSetStepFactory : RubyScriptStepFactory<NuixRemoveFromItemSet, Unit>
{
    private NuixRemoveFromItemSetStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixRemoveFromItemSet, Unit> Instance { get; } =
        new NuixRemoveFromItemSetStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 0);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>() { NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override string FunctionName => "RemoveFromItemSet";

    /// <inheritdoc />
    public override string RubyFunctionText => @"

    log ""Searching for item set '#{itemSetNameArg}'""
    item_set = $current_case.findItemSetByName(itemSetNameArg)

    if item_set.nil?
      log(""Could not find item set '#{itemSetNameArg}'"", severity: :warn)
      return
    end

    remove_opts = { 'removeDuplicates' => removeDuplicatesArg }

    if searchArg.nil?
      all_items = item_set.get_items
      items_count = all_items.length
      item_set.remove_items(all_items, remove_opts)
      log ""Removed all items: #{items_count}""
    else
      searchOptions = searchOptionsArg.nil? ? {} : searchOptionsArg
      log(""Search options: #{searchOptions}"", severity: :trace)
      items = $current_case.search_unsorted(searchArg, searchOptions)
      log(""Search '#{searchArg}' returned #{items.length} items"", severity: :debug)
      to_remove = $utilities.get_item_utility.intersection(items, item_set.get_items)
      log(""Intersection of search results and item set is #{to_remove.length} items"", severity: :debug)
      if to_remove.length == 0
        log ""No items found to remove.""
      else
        item_set.remove_items(to_remove, remove_opts)
        log ""Removed items: #{to_remove.length}""
      end
    end
";
}

/// <summary>
/// Removes particular items from a Nuix item set.
/// </summary>
public sealed class NuixRemoveFromItemSet : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixRemoveFromItemSetStepFactory.Instance;

    /// <summary>
    /// The name of the item set to remove results from.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [RubyArgument("itemSetNameArg")]
    [Alias("ItemSet")]
    public IStep<StringStream> ItemSetName { get; set; } = null!;

    /// <summary>
    /// The search term to use for choosing which items to remove.
    /// </summary>
    [StepProperty(2)]
    [RubyArgument("searchArg")]
    [DefaultValueExplanation("All items will be removed.")]
    [Example("Tag:sushi")]
    [Alias("Search")]
    public IStep<StringStream>? SearchTerm { get; set; }

    /// <summary>
    /// Pass additional search options to nuix. Options available:
    ///   - defaultFields: field(s) to query against when not present in the search string.
    /// Please see the nuix API for <code>Case.searchUnsorted</code> for more details.
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("searchOptionsArg")]
    [DefaultValueExplanation("No search options provided")]
    public IStep<Entity>? SearchOptions { get; set; }

    /// <summary>
    /// If true (default), duplicates of (top-level and above-top-level) originals are
    /// removed. When false, only the found items are removed causing new originals
    /// to be chosen from the remaining duplicates.
    /// </summary>
    [StepProperty(4)]
    [RubyArgument("removeDuplicatesArg")]
    [DefaultValueExplanation("true")]
    public IStep<bool> RemoveDuplicates { get; set; } = new BoolConstant(true);
}

}
