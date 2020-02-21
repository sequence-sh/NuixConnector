using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using NuixClient.Orchestration.Conditions;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration.Processes
{
    /// <summary>
    /// Executes all sub-processes whose conditions are met.
    /// </summary>
    internal class Branch : Process
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
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public List<Process> Options { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

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
            var r = obj is Branch bp && (Conditions ?? Enumerable.Empty<Condition>()).SequenceEqual(bp.Conditions ?? Enumerable.Empty<Condition>())
                                                       && Options.SequenceEqual(bp.Options);

            return r;
        }

        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }

        internal override IEnumerable<string> GetArgumentErrors()
        {
            return Options.SelectMany(process => process.GetArgumentErrors());
        }
    }
}