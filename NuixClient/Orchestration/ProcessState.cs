using System.Collections.Generic;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// Represents the current state of a running process
    /// </summary>
    internal class ProcessState
    {
        /// <summary>
        /// Artifacts of this process
        /// </summary>
        internal Dictionary<string, object> Artifacts = new Dictionary<string, object>();
    }
}