using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process. Can contain one or more steps
    /// </summary>
    internal abstract class Process
    {
        /// <summary>
        /// Get argument errors
        /// </summary>
        /// <returns></returns>
        internal abstract IEnumerable<string> GetArgumentErrors();
        

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
        public abstract IAsyncEnumerable<ResultLine> Execute();

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