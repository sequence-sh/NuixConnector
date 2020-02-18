using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using NuixClient.enums;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which searches a case with a particular search string and adds all items it finds to a particular item set.
    /// Will create a new item set if one doesn't already exist.
    /// </summary>
    internal class AddToItemSetProcess : Process
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Search and add to item set '{ItemSetName}'";

        /// <summary>
        /// Execute this process
        /// </summary>
        /// <returns></returns>
        public override IAsyncEnumerable<ResultLine> Execute()
        {
            var r = OutsideScripting.AddToItemSet(CasePath, SearchTerm, ItemSetName, null, null, ItemSetDeduplication, ItemSetDescription, DeduplicateBy, CustodianRanking);

            return r;
        }

        /// <summary>
        /// The production set to add results to. Will be created if it doesn't already exist
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ItemSetName { get; set; }


        /// <summary>
        /// The term to search for
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 4)]
        public string SearchTerm { get; set; }

        /// <summary>
        /// The path of the case to search
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 5)]
        public string CasePath { get; set; }

        /// <summary>
        /// The means of deduplicating items by key and prioritizing originals in a tie-break.
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 6)]
        public ItemSetDeduplication ItemSetDeduplication { get; set; }

        /// <summary>
        /// The description of the item set as a string.
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 7)]
        public string ItemSetDescription { get; set; }

        /// <summary>
        /// Whether to deduplicate as a family or individual
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 8)]
        public DeduplicateBy DeduplicateBy { get; set; }

        /// <summary>
        /// A list of custodian names ordered from highest ranked to lowest ranked. If this parameter is present and the deduplication parameter has not been specified, MD5 Ranked Custodian is assumed.
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 9)]
        public string[]? CustodianRanking { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.



        public override bool Equals(object? obj)
        {
            var r = obj is AddToItemSetProcess isp && (Conditions ?? Enumerable.Empty<Condition>()).SequenceEqual(isp.Conditions ?? Enumerable.Empty<Condition>())
                                                         && ItemSetName == isp.ItemSetName
                                                         && SearchTerm == isp.SearchTerm
                                                         && CasePath == isp.CasePath
                                                         && ItemSetDeduplication == isp.ItemSetDeduplication
                                                         && ItemSetDescription == isp.ItemSetDescription
                                                         && DeduplicateBy == isp.DeduplicateBy
                                                         && (CustodianRanking??Enumerable.Empty<string>()).SequenceEqual(isp.CustodianRanking??Enumerable.Empty<string>());

            return r;
        }

        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }
    }
}