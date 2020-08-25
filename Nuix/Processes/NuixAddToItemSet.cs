using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.enums;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a particular item set.
    /// Will create a new item set if one doesn't already exist.
    /// </summary>
    public sealed class NuixAddToItemSetProcessFactory : RubyScriptProcessFactory<NuixAddToItemSet, Unit>
    {
        private NuixAddToItemSetProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixAddToItemSet, Unit> Instance { get; } = new NuixAddToItemSetProcessFactory();


        /// <inheritdoc />
        public override Version RequiredVersion { get; } = new Version(4, 0);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.ANALYSIS
        };
    }


    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a particular item set.
    /// Will create a new item set if one doesn't already exist.
    /// </summary>
    public sealed class NuixAddToItemSet : RubyScriptProcess
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixAddToItemSetProcessFactory.Instance;


        ///// <inheritdoc />
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public override string GetName() => "Search and add to item set";


        /// <summary>
        /// The item set to add results to. Will be created if it doesn't already exist.
        /// </summary>

        [Required]
        [RunnableProcessProperty]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ItemSetName { get; set; }


        /// <summary>
        /// The term to search for.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        public string SearchTerm { get; set; }

        /// <summary>
        /// The path of the case to search.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        public string CasePath { get; set; }

        /// <summary>
        /// The means of deduplicating items by key and prioritizing originals in a tie-break.
        /// </summary>
        [RunnableProcessProperty]
        public ItemSetDeduplication ItemSetDeduplication { get; set; }

        /// <summary>
        /// The description of the item set.
        /// </summary>

        [RunnableProcessProperty]
        public string? ItemSetDescription { get; set; }

        /// <summary>
        /// Whether to deduplicate as a family or individual.
        /// </summary>

        [RunnableProcessProperty]
        public DeduplicateBy DeduplicateBy { get; set; }

        /// <summary>
        /// A list of custodian names ordered from highest ranked to lowest ranked.
        /// If this parameter is present and the deduplication parameter has not been specified, MD5 Ranked Custodian is assumed.
        /// </summary>

        [RunnableProcessProperty]
        public List<string>? CustodianRanking { get; set; }


        /// <summary>
        /// How to order the items to be added to the item set.
        /// </summary>
        [RunnableProcessProperty]
        [Example("name ASC, item-date DESC")]
        public string? Order { get; set; }

        /// <summary>
        /// The maximum number of items to add to the item set.
        /// </summary>
        [RunnableProcessProperty]
        public int? Limit { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable


        /// <inheritdoc />
        internal override string ScriptText =>
            @"
    the_case = utilities.case_factory.open(pathArg)
    itemSet = the_case.findItemSetByName(itemSetNameArg)
    if(itemSet == nil)
        itemSetOptions = {}
        itemSetOptions[:deduplication] = deduplicationArg if deduplicationArg != nil
        itemSetOptions[:description] = descriptionArg if descriptionArg != nil
        itemSetOptions[:deduplicateBy] = deduplicateByArg if deduplicateByArg != nil
        itemSetOptions[:custodianRanking] = custodianRankingArg.split("","") if custodianRankingArg != nil
        itemSet = the_case.createItemSet(itemSetNameArg, itemSetOptions)

        puts ""Item Set Created""
    else
        puts ""Item Set Found""
    end

    puts ""Searching""
    searchOptions = {}
    searchOptions[:order] = orderArg if orderArg != nil
    searchOptions[:limit] = limitArg.to_i if limitArg != nil
    items = the_case.search(searchArg, searchOptions)
    puts ""#{items.length} found""
    itemSet.addItems(items)
    puts ""items added""
    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "AddToItemSet";



        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("searchArg", SearchTerm, false);
            yield return ("itemSetNameArg", ItemSetName, false);

            yield return ("deduplicationArg",
                ItemSetDeduplication != ItemSetDeduplication.Default ? ItemSetDeduplication.GetDescription() : null,
                true);

            yield return("descriptionArg", ItemSetDescription, true);

            yield return("deduplicateByArg",DeduplicateBy.GetDescription(), false);

            yield return("custodianRankingArg",
                CustodianRanking != null?
                    string.Join(",", CustodianRanking) : null, true);

            yield return ("orderArg", Order, true);
            yield return ("limitArg", Limit?.ToString(), true);
        }
    }
}