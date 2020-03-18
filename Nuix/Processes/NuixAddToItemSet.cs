using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.enums;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a particular item set.
    /// Will create a new item set if one doesn't already exist.
    /// </summary>
    public sealed class NuixAddToItemSet : RubyScriptProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "Search and add to item set";


        /// <summary>
        /// The item set to add results to. Will be created if it doesn't already exist.
        /// </summary>
        
        [Required]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ItemSetName { get; set; }


        /// <summary>
        /// The term to search for.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        public string SearchTerm { get; set; }

        /// <summary>
        /// The path of the case to search.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }

        /// <summary>
        /// The means of deduplicating items by key and prioritizing originals in a tie-break.
        /// </summary>
        [YamlMember(Order = 6)]
        public ItemSetDeduplication ItemSetDeduplication { get; set; }

        /// <summary>
        /// The description of the item set.
        /// </summary>
        
        [YamlMember(Order = 7)]
        public string? ItemSetDescription { get; set; }

        /// <summary>
        /// Whether to deduplicate as a family or individual.
        /// </summary>
        
        [YamlMember(Order = 8)]
        public DeduplicateBy DeduplicateBy { get; set; }

        /// <summary>
        /// A list of custodian names ordered from highest ranked to lowest ranked.
        /// If this parameter is present and the deduplication parameter has not been specified, MD5 Ranked Custodian is assumed.
        /// </summary>
        
        [YamlMember(Order = 9)]
        public List<string>? CustodianRanking { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
            //TODO validate search term
            //TODO check other argument validity
        }

        internal override string ScriptName => "AddToItemSet.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-s", SearchTerm);
            yield return ("-n", ItemSetName);

            //TODO order and limit
            
            if (ItemSetDeduplication != ItemSetDeduplication.Default)
            {
                yield return("-d", ItemSetDeduplication.GetDescription());
            }

            if (ItemSetDescription != null)
            {
                yield return("-r", ItemSetDescription);
            }

            if (DeduplicateBy != DeduplicateBy.Individual)
            {
                yield return("-b", DeduplicateBy.GetDescription());
            }

            if (CustodianRanking != null)
            {
                yield return("-c", string.Join(",", CustodianRanking));
            }
        }
    }
}