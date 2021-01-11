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
    log ""Searching""

    searchOptions = {}
    searchOptions[:order] = orderArg if orderArg != nil
    searchOptions[:limit] = limitArg.to_i if limitArg != nil

    items = currentCase.search(searchArg, searchOptions)
    log ""#{items.length} items found""

    productionSet = currentCase.findProductionSetByName(productionSetNameArg)
    if(productionSet == nil)
        options = {}
        options[:description] = descriptionArg.to_i if descriptionArg != nil
        productionSet = currentCase.newProductionSet(productionSetNameArg, options)

        if productionProfileNameArg != nil
            productionSet.setProductionProfile(productionProfileNameArg)
        elsif productionProfilePathArg != nil
            profileBuilder = $utilities.getProductionProfileBuilder()
            profile = profileBuilder.load(productionProfilePathArg)

            if profile == nil
                log ""Could not find processing profile at #{productionProfilePathArg}""
                exit
            end

            productionSet.setProductionProfileObject(profile)
        else
            log 'No production profile set'
            exit
        end

        log ""Production Set Created""
    else
        log ""Production Set Found""
    end

    if items.length > 0
        productionSet.addItems(items)
        log ""Items added to production set""
    else
        log ""No items to add to production Set""
    end";
}

/// <summary>
/// Searches a case with a particular search string and adds all items it finds to a production set.
/// Will create a new production set if one with the given name does not already exist.
/// </summary>
[Alias("NuixCreateProductionSet")]
public sealed class NuixAddToProductionSet : RubyScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAddToProductionSetStepFactory.Instance;

    /// <summary>
    /// The term to search for
    /// </summary>
    [Required]
    [StepProperty(1)]
    [RubyArgument("searchArg", 1)]
    [Alias("Search")]
    public IStep<StringStream> SearchTerm { get; set; } = null!;

    /// <summary>
    /// The production set to add results to. Will be created if it doesn't already exist
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("productionSetNameArg", 2)]
    [Alias("ProductionSet")]
    public IStep<StringStream> ProductionSetName { get; set; } = null!;

    /// <summary>
    /// Description of the production set.
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("descriptionArg", 3)]
    [DefaultValueExplanation("No description")]
    public IStep<StringStream>? Description { get; set; }

    /// <summary>
    /// The name of the Production profile to use.
    /// Either this or the ProductionProfilePath must be set
    /// </summary>

    [RequiredVersion("Nuix", "7.2")]
    [StepProperty(4)]
    [Example("MyProcessingProfile")]
    [DefaultValueExplanation("The default processing profile will be used.")]
    [RubyArgument("productionProfileNameArg", 4)]
    [Alias("Profile")]
    public IStep<StringStream>? ProductionProfileName { get; set; }

    /// <summary>
    /// The path to the Production profile to use.
    /// Either this or the ProductionProfileName must be set.
    /// </summary>
    [RequiredVersion("Nuix", "7.6")]
    [StepProperty(5)]
    [Example("C:/Profiles/MyProcessingProfile.xml")]
    [DefaultValueExplanation("The default processing profile will be used.")]
    [RubyArgument("productionProfilePathArg", 5)]
    [Alias("ProfilePath")]
    public IStep<StringStream>? ProductionProfilePath { get; set; }

    /// <summary>
    /// How to order the items to be added to the production set.
    /// </summary>
    [StepProperty(6)]
    [Example("name ASC, item-date DESC")]
    [RubyArgument("orderArg", 6)]
    [DefaultValueExplanation("Default order")]
    public IStep<StringStream>? Order { get; set; }

    /// <summary>
    /// The maximum number of items to add to the production set.
    /// </summary>
    [StepProperty(7)]
    [RubyArgument("limitArg", 7)]
    [DefaultValueExplanation("No limit")]
    public IStep<int>? Limit { get; set; }

    /// <inheritdoc />
    public override Result<Unit, IError> VerifyThis(ISettings settings)
    {
        if (ProductionProfileName != null && ProductionProfilePath != null)
            return new SingleError(
                new StepErrorLocation(this),
                ErrorCode.ConflictingParameters,
                nameof(ProductionProfileName),
                nameof(ProductionProfilePath)
            );

        if (ProductionProfileName == null && ProductionProfilePath == null)
            return new SingleError(
                new StepErrorLocation(this),
                ErrorCode.MissingParameter,
                nameof(ProductionProfileName),
                nameof(ProductionProfilePath)
            );

        return base.VerifyThis(settings);
    }
}

}
