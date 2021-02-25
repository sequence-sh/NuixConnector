using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Searches a case with a particular search string and adds all items it finds to a production set.
/// Will create a new production set if one with the given name does not already exist.
/// </summary>
public sealed class
    NuixAddToProductionSetStepFactory : RubyScriptStepFactory<NuixAddToProductionSet, Unit>
{
    private NuixAddToProductionSetStepFactory() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static RubyScriptStepFactory<NuixAddToProductionSet, Unit> Instance { get; } =
        new NuixAddToProductionSetStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion => new(7, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>() { NuixFeature.PRODUCTION_SET };

    /// <inheritdoc />
    public override string FunctionName => "AddToProductionSet";

    /// <inheritdoc />
    public override string RubyFunctionText => @"

    log ""Searching for items to add: #{searchArg}""

    searchOptions = searchOptionsArg.nil? ? {} : searchOptionsArg
    log(""Search options: #{searchOptions}"", severity: :trace)

    if sortArg.nil?
      log('Search results will be unsorted', severity: :trace)
      items = $current_case.search_unsorted(searchArg, searchOptions)
    else
      log('Search results will be sorted', severity: :trace)
      items = $current_case.search(searchArg, searchOptions)
    end

    if items.length == 0
      log 'No items found. Nothing to add to production set.'
      return
    end

    log ""Items found: #{items.length}""

    productionSet = $current_case.findProductionSetByName(productionSetNameArg)

    if productionSet.nil?
      log ""Production set '#{productionSetNameArg}' not found. Creating.""
      options = {}
      options[:description] = descriptionArg.to_i if descriptionArg != nil
      log(""Production set options: #{options}"", severity: :trace)
      productionSet = $current_case.newProductionSet(productionSetNameArg, options)

      if productionProfileNameArg != nil
        log(""Setting production profile: #{productionProfileNameArg}"", severity: :debug)
        productionSet.setProductionProfile(productionProfileNameArg)
      elsif productionProfilePathArg != nil
        log(""Loading production profile from #{productionProfilePathArg}"", severity: :debug)
        profileBuilder = $utilities.getProductionProfileBuilder()
        profile = profileBuilder.load(productionProfilePathArg)
        if profile.nil?
          write_error(""Could not find processing profile: #{productionProfilePathArg}"", terminating: true)
        end
        productionSet.setProductionProfileObject(profile)
      else
        write_error(""No production profile set"", terminating: true)
      end
      log ""Successfully created '#{productionSetNameArg}' production set.""
    else
      log ""Existing production set '#{productionSetNameArg}' found""
    end

    log ""Adding #{items.length} items to production set '#{productionSetNameArg}'""
    productionSet.addItems(items)
    log('Finished adding items', severity: :debug)
";
}

/// <summary>
/// Searches a case with a particular search string and adds all items it finds to a production set.
/// Will create a new production set if one with the given name does not already exist.
/// </summary>
[Alias("NuixCreateProductionSet")]
public sealed class NuixAddToProductionSet : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAddToProductionSetStepFactory.Instance;

    /// <summary>
    /// The term to search for
    /// </summary>
    [Required]
    [StepProperty(1)]
    [RubyArgument("searchArg")]
    [Alias("Search")]
    public IStep<StringStream> SearchTerm { get; set; } = null!;

    /// <summary>
    /// The production set to add results to. Will be created if it doesn't already exist
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("productionSetNameArg")]
    [Alias("ProductionSet")]
    public IStep<StringStream> ProductionSetName { get; set; } = null!;

    /// <summary>
    /// Description of the production set.
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("descriptionArg")]
    [DefaultValueExplanation("No description")]
    [Alias("Description")]
    public IStep<StringStream>? ProductionSetDescription { get; set; }

    /// <summary>
    /// The name of the Production profile to use.
    /// Either this or the ProductionProfilePath must be set
    /// </summary>
    [RequiredVersion("Nuix", "7.2")]
    [StepProperty(4)]
    [Example("MyProcessingProfile")]
    [DefaultValueExplanation("If not set, the profile path will be used.")]
    [RubyArgument("productionProfileNameArg")]
    [Alias("Profile")]
    public IStep<StringStream>? ProductionProfileName { get; set; }

    /// <summary>
    /// The path to the Production profile to use.
    /// Either this or the ProductionProfileName must be set.
    /// </summary>
    [RequiredVersion("Nuix", "7.6")]
    [StepProperty(5)]
    [Example("C:/Profiles/MyProcessingProfile.xml")]
    [DefaultValueExplanation("If not set, the profile name will be used.")]
    [RubyArgument("productionProfilePathArg")]
    [Alias("ProfilePath")]
    public IStep<StringStream>? ProductionProfilePath { get; set; }

    /// <summary>
    /// Pass additional search options to nuix. For an unsorted search (default)
    /// the only available option is defaultFields. When using <code>SortSearch=true</code>
    /// the options are defaultFields, order, and limit.
    /// Please see the nuix API for <code>Case.search</code>
    /// and <code>Case.searchUnsorted</code> for more details.
    /// </summary>
    [RequiredVersion("Nuix", "7.0")]
    [StepProperty(6)]
    [RubyArgument("searchOptionsArg")]
    [DefaultValueExplanation("No search options provided")]
    public IStep<Core.Entity>? SearchOptions { get; set; }

    /// <summary>
    /// By default the search is not sorted by relevance which
    /// increases performance. Set this to true to sort the
    /// search by relevance.
    /// </summary>
    [RequiredVersion("Nuix", "7.0")]
    [StepProperty(7)]
    [RubyArgument("sortArg")]
    [DefaultValueExplanation("false")]
    public IStep<bool>? SortSearch { get; set; }

    /// <inheritdoc />
    public override Result<Unit, IError> VerifyThis(SCLSettings settings)
    {
        if (ProductionProfileName != null && ProductionProfilePath != null)
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.ConflictingParameters,
                nameof(ProductionProfileName),
                nameof(ProductionProfilePath)
            );

        if (ProductionProfileName == null && ProductionProfilePath == null)
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.MissingParameter,
                nameof(ProductionProfileName),
                nameof(ProductionProfilePath)
            );

        return base.VerifyThis(settings);
    }
}

}
