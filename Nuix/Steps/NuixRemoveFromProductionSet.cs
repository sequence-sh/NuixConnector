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
    : RubyScriptStepFactory<NuixRemoveFromProductionSet, Unit>
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

    log ""Searching""

    productionSet = $currentCase.findProductionSetByName(productionSetNameArg)
    if(productionSet == nil)
        log ""Production Set Not Found""
    else
        log ""Production Set Found""

        if searchArg != nil
            items = $currentCase.searchUnsorted(searchArg)
            productionSetItems = productionSet.getItems();
            itemsToRemove = items.to_a & productionSetItems
            productionSet.removeItems(itemsToRemove)
            log ""#{itemsToRemove.length} removed""

        else
            previousTotal = getItems().length

            productionSet.removeAllItems()
            log ""All items (#{previousTotal}) removed""
        end
    end";
}

/// <summary>
/// Removes particular items from a Nuix production set.
/// </summary>
public sealed class NuixRemoveFromProductionSet : RubyScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixRemoveFromProductionSetStepFactory.Instance;

    /// <summary>
    /// The production set to remove results from.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [RubyArgument("productionSetNameArg", 1)]
    [Alias("ProductionSet")]
    public IStep<StringStream> ProductionSetName { get; set; } = null!;

    /// <summary>
    /// The search term to use for choosing which items to remove.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("All items will be removed.")]
    [Example("Tag:sushi")]
    [RubyArgument("searchArg", 2)]
    [Alias("Search")]
    public IStep<StringStream>? SearchTerm { get; set; }
}

}
