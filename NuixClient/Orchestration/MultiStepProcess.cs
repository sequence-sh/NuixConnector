using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process containing multiple steps
    /// </summary>
    internal class MultiStepProcess : Process
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName()
        {
            return string.Join(" then ", Steps.Select(s=>s.GetName()));
        }

        /// <summary>
        /// Steps that make up this process. To be executed in order
        /// </summary>
        [Required]
        [DataMember]
        [JsonProperty(Order = 3)]
        public List<Process> Steps { get; set; }

        /// <summary>
        /// Execute the steps in this process until a condition is not met or a step fails 
        /// </summary>
        /// <returns></returns>
        public override async IAsyncEnumerable<ResultLine> Execute() 
        {
            foreach (var process in Steps)
            {
                foreach (var processCondition in process.Conditions?? Enumerable.Empty<Condition>())
                {
                    if (processCondition.IsMet())
                        yield return new ResultLine(true, processCondition.GetDescription());
                    else
                    {
                        yield return new ResultLine(false, $"CONDITION NOT MET: [{processCondition.GetDescription()}]");
                        yield break;
                    }
                }

                yield return new ResultLine(true, $"Executing '{process.GetName()}'");
                var allGood = true;
                var resultLines = process.Execute();
                await foreach (var resultLine in resultLines)
                {
                    yield return resultLine;
                    allGood &= resultLine.IsSuccess;
                }
                if(!allGood)
                    yield break;
            }
        }

        public override bool Equals(object? obj)
        {
            var r = obj is MultiStepProcess msp && (Conditions??Enumerable.Empty<Condition>()).SequenceEqual(msp.Conditions??Enumerable.Empty<Condition>())
                                                       && Steps.SequenceEqual(msp.Steps);

            return r;
        }

        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }
    }
}