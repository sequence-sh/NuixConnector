using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration
{
    internal class ListEnumeration : Enumeration
    {
        internal override string Name => $"[{string.Join(", ", List)}]";

        /// <summary>
        /// The elements to iterate over
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public List<string> List { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        internal override IEnumerable<string> Elements => List;

    }
}