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
        public override Version RequiredVersion =>  new Version(7, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };

    }

    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a production set.
    /// Will create a new production set if one with the given name does not already exist.
    /// </summary>
    public sealed class NuixAddToProductionSet : RubyScriptProcess
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixAddToProductionSetProcessFactory.Instance;


        ///// <inheritdoc />
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public override string GetName() => $"Search and add to production set.";

        /// <summary>
        /// The production set to add results to. Will be created if it doesn't already exist
        /// </summary>
        [Required]
        [RunnableProcessProperty]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IRunnableProcess<string> ProductionSetName { get; set; }


        /// <summary>
        /// The term to search for
        /// </summary>

        [Required]
        [RunnableProcessProperty]
        public IRunnableProcess<string> SearchTerm { get; set; }

        /// <summary>
        /// The path of the case to search
        /// </summary>

        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        public IRunnableProcess<string> CasePath { get; set; }

        /// <summary>
        /// Description of the production set.
        /// </summary>
        [RunnableProcessProperty]
        public IRunnableProcess<string>? Description { get; set; }

        /// <summary>
        /// The name of the Production profile to use.
        /// Either this or the ProductionProfilePath must be set
        /// </summary>

        [RequiredVersion("Nuix", "7.2")]
        [RunnableProcessProperty]
        [Example("MyProcessingProfile")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        public IRunnableProcess<string>? ProductionProfileName { get; set; }

        /// <summary>
        /// The path to the Production profile to use.
        /// Either this or the ProductionProfileName must be set.
        /// </summary>
        [RequiredVersion("Nuix", "7.6")]
        [RunnableProcessProperty]
        [Example("C:/Profiles/MyProcessingProfile.xml")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        public IRunnableProcess<string>? ProductionProfilePath { get; set; }

        /// <summary>
        /// How to order the items to be added to the production set.
        /// </summary>
        [RunnableProcessProperty]
        [Example("name ASC, item-date DESC")]
        public IRunnableProcess<string>? Order { get; set; }

        /// <summary>
        /// The maximum number of items to add to the production set.
        /// </summary>
        [RunnableProcessProperty]
        public IRunnableProcess<int>? Limit { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("searchArg", SearchTerm, false);
            yield return ("productionSetNameArg", ProductionSetName, false);
            yield return ("descriptionArg", Description, true);
            yield return ("productionProfileNameArg", ProductionProfileName, true);
            yield return ("productionProfilePathArg", ProductionProfilePath, true);
            yield return ("orderArg", Order, true);
            yield return ("limitArg", Limit, true);
        }

        /// <inheritdoc />
        internal override string ScriptText =>
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

        /// <inheritdoc />
        internal override string MethodName => "AddToProductionSet";

        /// <inheritdoc />
        public override Version? RunTimeNuixVersion => ProductionProfilePath == null? new Version(7, 2) : new Version(7,6);


        /// <inheritdoc />
        public override Result<Unit, IRunErrors> VerifyThis
        {
            get
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

                return Unit.Default;
            }
        }



    }
}