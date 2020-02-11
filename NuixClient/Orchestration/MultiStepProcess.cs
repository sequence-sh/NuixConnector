using System.Collections.Generic;
using System.Linq;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process containing multiple steps
    /// </summary>
    public class MultiStepProcess : IProcess
    {
        /// <summary>
        /// Create a new multi-step process
        /// </summary>
        /// <param name="name">The name of this process</param>
        /// <param name="steps"></param>
        /// <param name="conditions">Conditions which must be met for this process to be executed</param>
        public MultiStepProcess(string name, IEnumerable<IProcess> steps, IEnumerable<ICondition> conditions)
        {
            Name = name;
            Steps = steps.ToList();
            Conditions = conditions.ToList();
        }

        /// <summary>
        /// The name of this process
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Execute the steps in this process until a condition is not met or a step fails 
        /// </summary>
        /// <returns></returns>
        public async IAsyncEnumerable<ResultLine> Execute() 
        {

            foreach (var process in Steps)
            {
                foreach (var processCondition in process.Conditions?? Enumerable.Empty<ICondition>())
                {
                    if (processCondition.IsMet())
                        yield return new ResultLine(true, processCondition.Description);
                    else
                    {
                        yield return new ResultLine(false, $"CONDITION NOT MET: [{processCondition.Description}]");
                        yield break;
                    }
                }

                yield return new ResultLine(true, $"Executing '{process.Name}'");
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

        /// <summary>
        /// This process should only be executed if these conditions are met
        /// </summary>
        public IReadOnlyCollection<ICondition> Conditions { get; }

        /// <summary>
        /// Steps that make up this process. To be executed in order
        /// </summary>
        public List<IProcess> Steps { get; }
    }
}