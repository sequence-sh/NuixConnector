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
}
