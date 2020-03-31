using System.Collections.Generic;
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

    }
    /// <summary>
    /// Settings for a nuix process.
    /// </summary>
    public class NuixProcessSettings : INuixProcessSettings
    {
        /// <summary>
        /// Create a new NuixProcessSettings.
        /// </summary>
        /// <param name="useDongle"></param>
        /// <param name="nuixExeConsolePath"></param>
        public NuixProcessSettings(bool useDongle, string nuixExeConsolePath)
        {
            UseDongle = useDongle;
            NuixExeConsolePath = nuixExeConsolePath;
        }
        /// <summary>
        /// Whether to use a dongle for nuix authentication.
        /// </summary>
        public bool UseDongle { get; }
        /// <summary>
        /// The path to the nuix console executable.
        /// </summary>
        public string NuixExeConsolePath { get; }
    }

    internal class NuixProcessSettingsComparer : IEqualityComparer<INuixProcessSettings>
    {
        private NuixProcessSettingsComparer()
        {
        }

        public static NuixProcessSettingsComparer Instance = new NuixProcessSettingsComparer();

        /// <inheritdoc />
        public bool Equals(INuixProcessSettings x, INuixProcessSettings y)
        {
            if (x == null || y == null) return x == y;
            
            return x.UseDongle == y.UseDongle && x.NuixExeConsolePath == y.NuixExeConsolePath;
        }

        /// <inheritdoc />
        public int GetHashCode(INuixProcessSettings obj)
        {
            return System.HashCode.Combine(obj.UseDongle, obj.NuixExeConsolePath);
        }
    }
}
