using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a production set.
    /// Will create a new production set if one with the given name does not already exist.
    /// </summary>
    public sealed class NuixAddToProductionSetProcessFactory : RubyScriptProcessFactory<NuixAddToProductionSet, Unit>
    {
        private NuixAddToProductionSetProcessFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static RubyScriptProcessFactory<NuixAddToProductionSet, Unit> Instance { get; } = new NuixAddToProductionSetProcessFactory();


        /// <inheritdoc />
        public override Version RequiredNuixVersion =>  new Version(7, 2);

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
    the_case = utilities.case_factory.open(pathArg)
    puts ""Searching""

    searchOptions = {}
    searchOptions[:order] = orderArg if orderArg != nil
    searchOptions[:limit] = limitArg.to_i if limitArg != nil

    items = the_case.search(searchArg, searchOptions)
    puts ""#{items.length} items found""

    productionSet = the_case.findProductionSetByName(productionSetNameArg)
    if(productionSet == nil)
        options = {}
        options[:description] = descriptionArg.to_i if descriptionArg != nil
        productionSet = the_case.newProductionSet(productionSetNameArg, options)

        if productionProfileNameArg != nil
            productionSet.setProductionProfile(productionProfileNameArg)
        elsif productionProfilePathArg != nil
            profileBuilder = utilities.getProductionProfileBuilder()
            profile = profileBuilder.load(productionProfilePathArg)

            if profile == nil
                puts ""Could not find processing profile at #{productionProfilePathArg}""
                exit
            end

            productionSet.setProductionProfileObject(profile)
        else
            puts 'No production profile set'
            exit
        end

        puts ""Production Set Created""
    else
        puts ""Production Set Found""
    end

    if items.length > 0
        productionSet.addItems(items)
        puts ""Items added to production set""
    else
        puts ""No items to add to production Set""
    end

    the_case.close";

    }

    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a production set.
    /// Will create a new production set if one with the given name does not already exist.
    /// </summary>
    public sealed class NuixAddToProductionSet : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixAddToProductionSetProcessFactory.Instance;


        /// <summary>
        /// The path of the case to search
        /// </summary>

        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The term to search for
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [RubyArgument("searchArg", 2)]
        public IRunnableProcess<string> SearchTerm { get; set; } = null!;

        /// <summary>
        /// The production set to add results to. Will be created if it doesn't already exist
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [RubyArgument("productionSetNameArg", 3)]
        public IRunnableProcess<string> ProductionSetName { get; set; } = null!;

        /// <summary>
        /// Description of the production set.
        /// </summary>
        [RunnableProcessProperty]
        [RubyArgument("descriptionArg", 4)]
        public IRunnableProcess<string>? Description { get; set; }

        /// <summary>
        /// The name of the Production profile to use.
        /// Either this or the ProductionProfilePath must be set
        /// </summary>

        [RequiredVersion("Nuix", "7.2")]
        [RunnableProcessProperty]
        [Example("MyProcessingProfile")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        [RubyArgument("productionProfileNameArg", 5)]
        public IRunnableProcess<string>? ProductionProfileName { get; set; }

        /// <summary>
        /// The path to the Production profile to use.
        /// Either this or the ProductionProfileName must be set.
        /// </summary>
        [RequiredVersion("Nuix", "7.6")]
        [RunnableProcessProperty]
        [Example("C:/Profiles/MyProcessingProfile.xml")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        [RubyArgument("productionProfilePathArg", 6)]
        public IRunnableProcess<string>? ProductionProfilePath { get; set; }

        /// <summary>
        /// How to order the items to be added to the production set.
        /// </summary>
        [RunnableProcessProperty]
        [Example("name ASC, item-date DESC")]
        [RubyArgument("orderArg", 7)]
        public IRunnableProcess<string>? Order { get; set; }

        /// <summary>
        /// The maximum number of items to add to the production set.
        /// </summary>
        [RunnableProcessProperty]
        [RubyArgument("limitArg", 8)]
        public IRunnableProcess<int>? Limit { get; set; }


        /// <inheritdoc />
        public override Result<Unit, IRunErrors> VerifyThis(IProcessSettings settings)
        {
            if (ProductionProfileName != null && ProductionProfilePath != null)
                return new RunError(
                    $"Only one of {nameof(ProductionProfileName)} and {nameof(ProductionProfilePath)} may be set.",
                    Name,
                    null,
                    ErrorCode.ConflictingParameters);

            if (ProductionProfileName == null && ProductionProfilePath == null)
                return new RunError(
                    $"Either {nameof(ProductionProfileName)} or {nameof(ProductionProfilePath)} must be set.",
                    Name,
                    null,
                    ErrorCode.MissingParameter);

            return base.VerifyThis(settings);
        }
    }
}