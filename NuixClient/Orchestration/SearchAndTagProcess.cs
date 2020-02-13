using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which searches a case with a particular search string and tags all files it finds
    /// </summary>
    internal class SearchAndTagProcess : Process
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Search and Tag with '{Tag}'";

        /// <summary>
        /// Execute this process
        /// </summary>
        /// <returns></returns>
        public override IAsyncEnumerable<ResultLine> Execute()
        {
            var r = OutsideScripting.SearchAndTag(CasePath, SearchTerm, Tag);

            return r;
        }

        /// <summary>
        /// The tag to assign to found results
        /// </summary>
        [DataMember]
        [Required]
        [JsonProperty(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Tag { get; set; }


        /// <summary>
        /// The term to search for
        /// </summary>
        [DataMember]
        [Required]
        [JsonProperty(Order = 4)]
        public string SearchTerm { get; set; }

        /// <summary>
        /// The path of the case to search
        /// </summary>
        [DataMember]
        [Required]
        [JsonProperty(Order = 5)]
        public string CasePath { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.



        public override bool Equals(object? obj)
        {
            var r = obj is SearchAndTagProcess stp && (Conditions ?? Enumerable.Empty<Condition>()).SequenceEqual(stp.Conditions ?? Enumerable.Empty<Condition>())
                                                        && Tag == stp.Tag
                                                        && SearchTerm == stp.SearchTerm
                                                        && CasePath == stp.CasePath;

            return r;
        }

        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }
    }
}