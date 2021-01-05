using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a production set.
    /// Will create a new production set if one with the given name does not already exist.
    /// </summary>
    public sealed class NuixAddToProductionSetStepFactory : RubyScriptStepFactory<NuixAddToProductionSet, Unit>
    {
        private NuixAddToProductionSetStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static RubyScriptStepFactory<NuixAddToProductionSet, Unit> Instance { get; } = new NuixAddToProductionSetStepFactory();


        /// <inheritdoc />
        public override Version RequiredNuixVersion =>  new(7, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };

        /// <inheritdoc />
        public override string FunctionName => "AddToProductionSet";

        /// <inheritdoc />
        public override string RubyFunctionText =>
            @"
    the_case = $utilities.case_factory.open(pathArg)
    log ""Searching""

    searchOptions = {}
    searchOptions[:order] = orderArg if orderArg != nil
    searchOptions[:limit] = limitArg.to_i if limitArg != nil

    items = the_case.search(searchArg, searchOptions)
    log ""#{items.length} items found""

    productionSet = the_case.findProductionSetByName(productionSetNameArg)
    if(productionSet == nil)
        options = {}
        options[:description] = descriptionArg.to_i if descriptionArg != nil
        productionSet = the_case.newProductionSet(productionSetNameArg, options)

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
    end

    the_case.close";

    }

    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a production set.
    /// Will create a new production set if one with the given name does not already exist.
    /// </summary>
    public sealed class NuixAddToProductionSet : RubyScriptStepBase<Unit>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixAddToProductionSetStepFactory.Instance;


        /// <summary>
        /// The path of the case to search
        /// </summary>

        [Required]
        [StepProperty(1)]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<StringStream> CasePath { get; set; } = null!;

        /// <summary>
        /// The term to search for
        /// </summary>
        [Required]
        [StepProperty(2)]
        [RubyArgument("searchArg", 2)]
        public IStep<StringStream> SearchTerm { get; set; } = null!;

        /// <summary>
        /// The production set to add results to. Will be created if it doesn't already exist
        /// </summary>
        [Required]
        [StepProperty(3)]
        [RubyArgument("productionSetNameArg", 3)]
        public IStep<StringStream> ProductionSetName { get; set; } = null!;

        /// <summary>
        /// Description of the production set.
        /// </summary>
        [StepProperty(4)]
        [RubyArgument("descriptionArg", 4)]
        [DefaultValueExplanation("No description")]
        public IStep<StringStream>? Description { get; set; }

        /// <summary>
        /// The name of the Production profile to use.
        /// Either this or the ProductionProfilePath must be set
        /// </summary>

        [RequiredVersion("Nuix", "7.2")]
        [StepProperty(5)]
        [Example("MyProcessingProfile")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        [RubyArgument("productionProfileNameArg", 5)]
        public IStep<StringStream>? ProductionProfileName { get; set; }

        /// <summary>
        /// The path to the Production profile to use.
        /// Either this or the ProductionProfileName must be set.
        /// </summary>
        [RequiredVersion("Nuix", "7.6")]
        [StepProperty(6)]
        [Example("C:/Profiles/MyProcessingProfile.xml")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        [RubyArgument("productionProfilePathArg", 6)]
        public IStep<StringStream>? ProductionProfilePath { get; set; }

        /// <summary>
        /// How to order the items to be added to the production set.
        /// </summary>
        [StepProperty(7)]
        [Example("name ASC, item-date DESC")]
        [RubyArgument("orderArg", 7)]
        [DefaultValueExplanation("Default order")]
        public IStep<StringStream>? Order { get; set; }

        /// <summary>
        /// The maximum number of items to add to the production set.
        /// </summary>
        [StepProperty(8)]
        [RubyArgument("limitArg", 8)]
        [DefaultValueExplanation("No limit")]
        public IStep<int>? Limit { get; set; }


        /// <inheritdoc />
        public override Result<Unit, IError> VerifyThis(ISettings settings)
        {
            if (ProductionProfileName != null && ProductionProfilePath != null)
                return new SingleError(
                    $"Only one of {nameof(ProductionProfileName)} and {nameof(ProductionProfilePath)} may be set.",
                    ErrorCode.ConflictingParameters,
                    new StepErrorLocation(this)
                    );

            if (ProductionProfileName == null && ProductionProfilePath == null)
                return new SingleError(
                    $"Either {nameof(ProductionProfileName)} or {nameof(ProductionProfilePath)} must be set.",
                    ErrorCode.MissingParameter,
                    new StepErrorLocation(this));

            return base.VerifyThis(settings);
        }
    }
}