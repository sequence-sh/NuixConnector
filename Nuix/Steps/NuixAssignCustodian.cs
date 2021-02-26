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
/// Searches a NUIX case with a particular search string and assigns all files it finds to a particular custodian.
/// </summary>
public sealed class NuixAssignCustodianFactory : RubyScriptStepFactory<NuixAssignCustodian, Unit>
{
    private NuixAssignCustodianFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixAssignCustodian, Unit> Instance { get; } =
        new NuixAssignCustodianFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(5, 0);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
        = new List<NuixFeature> { NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override string FunctionName => "AssignCustodian";

    /// <inheritdoc />
    public override string RubyFunctionText => @"

    log ""Searching for items: #{searchArg}""

    searchOptions = searchOptionsArg.nil? ? {} : searchOptionsArg
    log(""Search options: #{searchOptions}"", severity: :trace)

    if sortArg.nil?
      log('Using unsorted search', severity: :trace)
      items = $current_case.search_unsorted(searchArg, searchOptions)
    else
      log('Using sorted search', severity: :trace)
      items = $current_case.search(searchArg, searchOptions)
    end

    if items.length == 0
      log 'No items found. Nothing to assign to custodian.'
      return
    end

    log ""Items found: #{items.length}""

    items_processed = 0
    $utilities.get_bulk_annotater.assign_custodian(custodianArg, items) {|item| items_processed += 1 }

    log ""#{items_processed} items assigned to custodian #{custodianArg}""
";
}

/// <summary>
/// Searches a NUIX case with a particular search string and assigns all files it finds to a particular custodian.
/// </summary>
public sealed class NuixAssignCustodian : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAssignCustodianFactory.Instance;

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
    /// The custodian to assign to found results.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("custodianArg")]
    public IStep<StringStream> Custodian { get; set; } = null!;

    /// <summary>
    /// Pass additional search options to nuix. For an unsorted search (default)
    /// the only available option is defaultFields. When using <code>SortSearch=true</code>
    /// the options are defaultFields, order, and limit.
    /// Please see the nuix API for <code>Case.search</code>
    /// and <code>Case.searchUnsorted</code> for more details.
    /// </summary>
    [RequiredVersion("Nuix", "7.0")]
    [StepProperty(3)]
    [RubyArgument("searchOptionsArg")]
    [DefaultValueExplanation("No search options provided")]
    public IStep<Entity>? SearchOptions { get; set; }

    /// <summary>
    /// By default the search is not sorted by relevance which
    /// increases performance. Set this to true to sort the
    /// search by relevance.
    /// </summary>
    [RequiredVersion("Nuix", "7.0")]
    [StepProperty(4)]
    [RubyArgument("sortArg")]
    [DefaultValueExplanation("false")]
    public IStep<bool>? SortSearch { get; set; }
}

}
