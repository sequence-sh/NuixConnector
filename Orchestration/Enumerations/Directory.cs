using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Orchestration.Enumerations
{
    /// <summary>
    /// Enumerates through files in a directory
    /// </summary>
    public class Directory : Enumeration
    {
        internal override string Name => $"'{Path}'";

        internal override IEnumerable<string> Elements
        {
            get
            {
                if (System.IO.Directory.Exists(Path))
                {
                    return System.IO.Directory.GetFiles(Path);
                }

                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// The path to the directory
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 1)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Path { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}