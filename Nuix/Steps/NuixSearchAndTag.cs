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
/// Searches a NUIX case with a particular search string and tags all files it finds.
/// </summary>
public sealed class NuixSearchAndTagStepFactory : RubyScriptStepFactory<NuixSearchAndTag, Unit>
{
    private NuixSearchAndTagStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixSearchAndTag, Unit> Instance { get; } =
        new NuixSearchAndTagStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 0);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
        = new List<NuixFeature> { NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override IReadOnlyCollection<IRubyHelper> RequiredHelpers { get; }
        = new List<IRubyHelper> { NuixSearch.Instance };

    /// <inheritdoc />
    public override string FunctionName => "SearchAndTag";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    items = search(searchArg, searchOptionsArg, sortArg)
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

    items_processed = 0
    $utilities.get_bulk_annotater.add_tag(tagArg, all_items) { items_processed += 1 }

    log ""Items tagged: #{items_processed}""
";
}

/// <summary>
/// Searches a NUIX case with a particular search string and tags all files it finds.
/// </summary>
public sealed class NuixSearchAndTag : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixSearchAndTagStepFactory.Instance;

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
    /// The tag to assign to found results.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("tagArg")]
    public IStep<StringStream> Tag { get; set; } = null!;

    /// <summary>
    /// Pass additional search options to nuix. For an unsorted search (default)
    /// the only available option is defaultFields. When using <code>SortSearch=true</code>
    /// the options are defaultFields, order, and limit.
    /// Please see the nuix API for <code>Case.search</code>
    /// and <code>Case.searchUnsorted</code> for more details.
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("searchOptionsArg")]
    [DefaultValueExplanation("No search options provided")]
    public IStep<Entity>? SearchOptions { get; set; }

    /// <summary>
    /// By default the search is not sorted by relevance which
    /// increases performance. Set this to true to sort the
    /// search by relevance.
    /// </summary>
    [StepProperty(4)]
    [RubyArgument("sortArg")]
    [DefaultValueExplanation("false")]
    public IStep<bool>? SortSearch { get; set; }

    /// <summary>
    /// Defines the type of search that is done. By default only the items
    /// responsive to the search terms are tagged, but the result set
    /// can be augmented using this parameter.
    /// </summary>
    [StepProperty(5)]
    [RubyArgument("searchTypeArg")]
    [DefaultValueExplanation("ItemsOnly")]
    public IStep<SearchType> SearchType { get; set; } =
        new EnumConstant<SearchType>(Enums.SearchType.ItemsOnly);
}

}
