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
/// Searches a NUIX case with a particular search string and tags all files it finds.
/// </summary>
public sealed class
    NuixSearchAndExcludeStepFactory : RubyScriptStepFactory<NuixSearchAndExclude, Unit>
{
    private NuixSearchAndExcludeStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixSearchAndExclude, Unit> Instance { get; } =
        new NuixSearchAndExcludeStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 0);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
        = new List<NuixFeature> { NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override string FunctionName => "SearchAndExclude";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    log ""Searching for '#{searchArg}'""

    searchOptions = searchOptionsArg.nil? ? {} : searchOptionsArg
    log(""Search options: #{searchOptions}"", severity: :trace)

    if sortArg.nil? || !sortArg
      log('Search results will be unsorted', severity: :trace)
      items = $current_case.search_unsorted(searchArg, searchOptions)
    else
      log('Search results will be sorted', severity: :trace)
      items = $current_case.search(searchArg, searchOptions)
    end

    log ""Items found: #{items.length}""

    return unless items.length > 0

    if searchTypeArg.eql? 'items'
      all_items = items
    else
      iutil = $utilities.get_item_utility
      case searchTypeArg
        when 'descendants'
          all_items = iutil.find_descendants(items)
          log ""Descendants found: #{all_items.count}""
        when 'families'
          all_items = iutil.find_families(items)
          log ""Family items found: #{all_items.count}""
        when 'items_descendants'
          all_items = iutil.find_items_and_descendants(items)
          log ""Items and descendants found: #{all_items.count}""
        when 'items_duplicates'
          all_items = iutil.find_items_and_duplicates(items)
          log ""Items and duplicates found: #{all_items.count}""
        when 'thread_items'
          all_items = iutil.find_thread_items(items)
          log ""Thread items found: #{all_items.count}""
        when 'toplevel_items'
          all_items = iutil.find_top_level_items(items)
          log ""Top-level items found: #{all_items.count}""
      end
    end
    
    unless tagArg.nil?
      items_tagged = 0
      $utilities.get_bulk_annotater.add_tag(tagArg, all_items) { items_tagged += 1 }
      log ""Items tagged: #{items_tagged}""
    end

    exclude_reason = exclusionArg.nil? ? searchArg : exclusionArg
    items_excluded = 0
    $utilities.get_bulk_annotater.exclude(exclude_reason, all_items) { items_excluded += 1 }
    log ""Items excluded: #{items_excluded}""
";
}

/// <summary>
/// Searches a NUIX case with a particular search string and tags all files it finds.
/// </summary>
public sealed class NuixSearchAndExclude : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixSearchAndExcludeStepFactory.Instance;

    /// <summary>
    /// The term to search for.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [Example("*.txt")]
    [RubyArgument("searchArg")]
    [Alias("Search")]
    public IStep<StringStream> SearchTerm { get; set; } = null!;

    /// <summary>
    /// The exclusion reason
    /// </summary>
    [StepProperty(2)]
    [RubyArgument("exclusionArg")]
    [DefaultValueExplanation("The SearchTerm is used as the exclusion reason")]
    [Alias("Exclusion")]
    public IStep<StringStream>? ExclusionReason { get; set; }

    /// <summary>
    /// The tag to assign to the excluded items
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("tagArg")]
    [DefaultValueExplanation("Items will not be tagged")]
    public IStep<StringStream>? Tag { get; set; }

    /// <summary>
    /// Pass additional search options to nuix. For an unsorted search (default)
    /// the only available option is defaultFields. When using <code>SortSearch=true</code>
    /// the options are defaultFields, order, and limit.
    /// Please see the nuix API for <code>Case.search</code>
    /// and <code>Case.searchUnsorted</code> for more details.
    /// </summary>
    [StepProperty(4)]
    [RubyArgument("searchOptionsArg")]
    [DefaultValueExplanation("No search options provided")]
    public IStep<Entity>? SearchOptions { get; set; }

    /// <summary>
    /// By default the search is not sorted by relevance which
    /// increases performance. Set this to true to sort the
    /// search by relevance.
    /// </summary>
    [StepProperty(5)]
    [RubyArgument("sortArg")]
    [DefaultValueExplanation("false")]
    public IStep<bool>? SortSearch { get; set; }

    /// <summary>
    /// Defines the type of search that is done. By default only the items
    /// responsive to the search terms are excluded, but the result set
    /// can be augmented using this parameter.
    /// </summary>
    [StepProperty(6)]
    [RubyArgument("searchTypeArg")]
    [DefaultValueExplanation("ItemsOnly")]
    public IStep<SearchType> SearchType { get; set; } =
        new EnumConstant<SearchType>(Enums.SearchType.ItemsOnly);
}

}
