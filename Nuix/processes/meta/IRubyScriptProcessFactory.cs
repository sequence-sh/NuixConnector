using System;
using System.Collections.Generic;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// The process factory for a ruby script process.
    /// </summary>
    public interface IRubyScriptProcessFactory : IRunnableProcessFactory
    {
        /// <summary>
        /// The required Nuix version for the process.
        /// </summary>
        Version RequiredVersion { get; }

        /// <summary>
        /// The required Nuix features.
        /// </summary>
        IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
    }
}