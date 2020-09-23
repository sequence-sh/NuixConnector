using System;

namespace Reductech.EDR.Connectors.Nuix.Processes.Meta
{
    /// <summary>
    /// Helper methods relating to Nuix Versions-
    /// </summary>
    public static class NuixVersionHelper
    {
        /// <summary>
        /// The default required version of Nuix.
        /// 5.0 - this is required to check the nuix features.
        /// </summary>
        public static Version DefaultRequiredVersion { get; } = new Version(5, 0);
    }
}