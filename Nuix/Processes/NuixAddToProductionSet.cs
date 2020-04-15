using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a production set.
    /// Will create a new production set if one with the given name does not already exist.
    /// </summary>
    public sealed class NuixAddToProductionSet : RubyScriptProcess
    {
        /// <inheritdoc />
        protected override NuixReturnType ReturnType => NuixReturnType.Unit;

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => $"Search and add to production set.";

        /// <summary>
        /// The production set to add results to. Will be created if it doesn't already exist
        /// </summary>
        [Required]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ProductionSetName { get; set; }


        /// <summary>
        /// The term to search for
        /// </summary>

        [Required]
        [YamlMember(Order = 4)]
        public string SearchTerm { get; set; }

        /// <summary>
        /// The path of the case to search
        /// </summary>

        [Required]
        [YamlMember(Order = 5)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }

        /// <summary>
        /// Description of the production set.
        /// </summary>
        [YamlMember(Order = 6)]
        public string? Description { get; set; }

        /// <summary>
        /// How to order the items to be added to the production set.
        /// </summary>
        [YamlMember(Order = 7)]
        [ExampleValue("name ASC, item-date DESC")]
        public string? Order { get; set; }

        /// <summary>
        /// The maximum number of items to add to the production set.
        /// </summary>
        [YamlMember(Order = 8)]
        public int? Limit { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)>
            GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("searchArg", SearchTerm, false);
            yield return ("productionSetNameArg", ProductionSetName, false);
            yield return ("descriptionArg", Description, true);
            yield return ("orderArg", Order, true);
            yield return ("limitArg", Limit?.ToString(), true);
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
        internal override Version RequiredVersion { get; } = new Version(3, 6);

        /// <inheritdoc />
        internal override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };

    }
}