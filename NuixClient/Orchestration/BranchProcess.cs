using System.Collections.Generic;
using System.Linq;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which executes all sub-processes whose conditions are met
    /// </summary>
    public class BranchProcess : Process
    {
        private readonly string _name;

        /// <summary>
        /// Create a new branch process
        /// </summary>
        /// <param name="name">The name of this process</param>
        /// <param name="options">Processes. Those whose conditions are met will be executed in order</param>
        /// <param name="conditions">Conditions which must be met for this process to be executed</param>
        public BranchProcess(string name, IEnumerable<Process> options, IEnumerable<Condition> conditions)
        {
            _name = name;
            Options = options;
            conditions.ToList();
        }

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

        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName()
        {
            return _name;
        }

        /// <summary>
        /// Processes which will be executed as part of this process if their conditions are met
        /// </summary>
        public IEnumerable<Process> Options { get; }
    }
}