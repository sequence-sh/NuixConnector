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
public sealed class NuixRemoveFromProductionSetStepFactory
    : RubySearchStepFactory<NuixRemoveFromProductionSet, Unit>
{
    private NuixRemoveFromProductionSetStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixRemoveFromProductionSet, Unit> Instance { get; } =
        new NuixRemoveFromProductionSetStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(4, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>() { NuixFeature.PRODUCTION_SET };

    /// <inheritdoc />
    public override string FunctionName => "RemoveFromProductionSet";

    /// <inheritdoc />
    public override string RubyFunctionText => @"

    log ""Searching for production set '#{productionSetNameArg}'""
    production_set = $current_case.findProductionSetByName(productionSetNameArg)

    if production_set.nil?
      log(""Could not find production set '#{productionSetNameArg}'"", severity: :warn)
      return
    end

    if searchArg.nil? or searchArg.to_s.empty?
      items_count = production_set.get_items.length
      production_set.remove_all_items
      log ""Removed all items: #{items_count}""
    else
      items = search(searchArg, searchOptionsArg, sortArg)
      items = expand_search(items, searchTypeArg)
      to_remove = $utilities.get_item_utility.intersection(items, production_set.get_items)
      log(""Intersection of search results and production set is #{to_remove.length} items"", severity: :debug)
      if to_remove.length == 0
        log ""No items found to remove.""
      else
        production_set.remove_items(to_remove)
        log ""Removed items: #{to_remove.length}""
      end
    end
";
}

/// <summary>
/// Removes particular items from a Nuix production set.
/// </summary>
public sealed class NuixRemoveFromProductionSet : RubySearchStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixRemoveFromProductionSetStepFactory.Instance;

    /// <summary>
    /// The production set to remove results from.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [RubyArgument("productionSetNameArg")]
    [Alias("ProductionSet")]
    public IStep<StringStream> ProductionSetName { get; set; } = null!;

    /// <summary>
    /// The search term to use for choosing which items to remove.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("All items removed.")]
    [Example("Tag:sushi")]
    [RubyArgument("searchArg")]
    [Alias("Search")]
    public override IStep<StringStream> SearchTerm { get; set; } = new StringConstant(string.Empty);
}

}
