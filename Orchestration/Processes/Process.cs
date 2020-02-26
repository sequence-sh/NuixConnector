using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Orchestration.Conditions;
using YamlDotNet.Serialization;

namespace Orchestration.Processes
{
    /// <summary>
    /// A process. Can contain one or more steps
    /// </summary>
    public abstract class Process
    {
        /// <summary>
        /// Get argument errors
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetArgumentErrors();
        

        /// <summary>
        /// Conditions which must be true for this process to be executed
        /// </summary>
        [YamlMember(Order = 1)]
        public List<Condition>? Conditions { get; set; }

        /// <summary>
        /// The name of this process
        /// </summary>
        public abstract string GetName();

        /// <summary>
        /// Executes this process. Should only be called if all conditions are met
        /// </summary>
        /// <returns></returns>
        public abstract IAsyncEnumerable<Result<string>> Execute();

        /// <summary>
        /// String representation of this process
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetName();
        }
    }
}