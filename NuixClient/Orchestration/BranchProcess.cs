using System.Collections.Generic;
using System.Linq;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which executes all sub-processes whose conditions are met
    /// </summary>
    public class BranchProcess : IProcess
    {
        /// <summary>
        /// Create a new branch process
        /// </summary>
        /// <param name="name">The name of this process</param>
        /// <param name="options">Processes. Those whose conditions are met will be executed in order</param>
        /// <param name="conditions">Conditions which must be met for this process to be executed</param>
        public BranchProcess(string name, IEnumerable<IProcess> options, IEnumerable<ICondition> conditions)
        {
            Name = name;
            Options = options;
            Conditions = conditions.ToList();
        }

        /// <summary>
        /// Execute this process.
        /// </summary>
        /// <returns></returns>
        public async IAsyncEnumerable<ResultLine> Execute()
        {
            foreach (var process in Options)
            {
                if (process.Conditions.All(c => c.IsMet()))
                {
                    yield return new ResultLine(true, $"Executing '{process.Name}'");
                    var results = process.Execute();

                    await foreach (var result in results)
                    {
                        yield return result;
                    }
                }
                else
                {
                    yield return new ResultLine(true, $"Not executing '{process.Name}'");
                }
            }
        }

        /// <summary>
        /// Conditions which should be met before this process is executed
        /// </summary>
        public IReadOnlyCollection<ICondition> Conditions { get; }

        /// <summary>
        /// The name of this process
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Processes which will be executed as part of this process if their conditions are met
        /// </summary>
        public IEnumerable<IProcess> Options { get; }
    }
}