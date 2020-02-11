using System.Collections.Generic;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process. Can contain one or more steps
    /// </summary>
    public interface IProcess
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Executes this process. Should only be called if all conditions are met
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerable<ResultLine> Execute();

        /// <summary>
        /// This process should only be executed if these conditions are met
        /// </summary>
        IReadOnlyCollection<ICondition>? Conditions { get; }
    }
}