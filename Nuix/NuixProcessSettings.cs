using System;
using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Utilities.Processes.mutable;

namespace Reductech.EDR.Connectors.Nuix
{
    /// <summary>
    /// Settings required to run a nuix process.
    /// </summary>
    public interface INuixProcessSettings : IProcessSettings
    {
        /// <summary>
        /// Whether to use a dongle for nuix authentication.
        /// </summary>
        bool UseDongle { get; }

        /// <summary>
        /// The path to the nuix console executable.
        /// </summary>
        string NuixExeConsolePath { get; }

        /// <summary>
        /// The version of Nuix
        /// </summary>
        Version NuixVersion { get; }

        /// <summary>
        /// A list of available Nuix features.
        /// </summary>
        IReadOnlyCollection<NuixFeature> NuixFeatures { get; }

    }
    /// <summary>
    /// Settings for a nuix process.
    /// </summary>
    public class NuixProcessSettings : INuixProcessSettings
    {
        /// <summary>
        /// Create a new NuixProcessSettings.
        /// </summary>
        public NuixProcessSettings(bool useDongle, string nuixExeConsolePath, Version nuixVersion, IReadOnlyCollection<NuixFeature> nuixFeatures)
        {
            UseDongle = useDongle;
            NuixExeConsolePath = nuixExeConsolePath;
            NuixVersion = nuixVersion;
            NuixFeatures = nuixFeatures;
        }
        /// <summary>
        /// Whether to use a dongle for nuix authentication.
        /// </summary>
        public bool UseDongle { get; }
        /// <summary>
        /// The path to the nuix console executable.
        /// </summary>
        public string NuixExeConsolePath { get; }

        /// <inheritdoc />
        public Version NuixVersion { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<NuixFeature> NuixFeatures { get; }
    }
}
