using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which executes all sub-processes whose conditions are met
    /// </summary>
    internal class BranchProcess : Process
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName()
        {
            return string.Join(" or ", Options.Select(s => s.GetName()));
        }


        /// <summary>
        /// Processes which will be executed as part of this process if their conditions are met
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 3)]
        public List<Process> Options { get; set; }

        /// <summary>
        /// Execute this process.
        /// </summary>
        /// <returns></returns>
        public override async IAsyncEnumerable<ResultLine> Execute()
        {
            foreach (var process in Options)
            {
                if (process.Conditions.All(c => c.IsMet()))
                {
                    yield return new ResultLine(true, $"Executing '{process.GetName()}'");
                    var results = process.Execute();

                    await foreach (var result in results)
                    {
                        yield return result;
                    }
                }
                else
                {
                    yield return new ResultLine(true, $"Not executing '{process.GetName()}'");
                }
            }
        }


        public override bool Equals(object? obj)
        {
            var r = obj is BranchProcess bp && (Conditions ?? Enumerable.Empty<Condition>()).SequenceEqual(bp.Conditions ?? Enumerable.Empty<Condition>())
                                                       && Options.SequenceEqual(bp.Options);

            return r;
        }

        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }
    }
}