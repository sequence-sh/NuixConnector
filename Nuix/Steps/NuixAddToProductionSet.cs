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
/// Run a search query in Nuix and add all items found to a production set.
/// Will create a new production set if one with the given name does not already exist.
/// </summary>
public sealed class
    NuixAddToProductionSetStepFactory : RubySearchStepFactory<NuixAddToProductionSet, Unit>
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

    items = search(searchArg, searchOptionsArg, sortArg)
    return unless items.length > 0

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

    all_items = expand_search(items, searchTypeArg)

    log ""Adding #{all_items.length} items to production set '#{productionSetNameArg}'""
    productionSet.addItems(all_items)
    log('Finished adding items', severity: :debug)
";
}

/// <summary>
/// Run a search query in Nuix and add all items found to a production set.
/// Will create a new production set if one with the given name does not already exist.
/// </summary>
[Alias("NuixCreateProductionSet")]
public sealed class NuixAddToProductionSet : RubySearchStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAddToProductionSetStepFactory.Instance;

    /// <summary>
    /// The production set to add results to. Will be created if it doesn't already exist.
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
