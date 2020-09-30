using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix
{
    /// <summary>
    /// Settings required to run a nuix step.
    /// </summary>
    public interface INuixSettings : ISettings
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
    /// Settings for a nuix step.
    /// </summary>
    public class NuixSettings : INuixSettings
    {
        /// <summary>
        /// Create a new NuixSettings.
        /// </summary>
        public NuixSettings(bool useDongle, string nuixExeConsolePath, Version nuixVersion, IReadOnlyCollection<NuixFeature> nuixFeatures)
        {
            UseDongle = useDongle;
            NuixExeConsolePath = nuixExeConsolePath;
            NuixVersion = nuixVersion;
            NuixFeatures = nuixFeatures;
        }

        /// <summary>
        /// Tries to create new Nuix Process Settings from the configuration manager.
        /// </summary>
        public static Result<NuixSettings> TryCreate(Func<string, string> getSetting)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var useDongleString =  getSetting("NuixUseDongle");
            var nuixExeConsolePath = getSetting("NuixExeConsolePath");
            var nuixVersionString = getSetting("NuixVersion");
            var nuixFeaturesString = getSetting("NuixFeatures");

            var stringBuilder = new StringBuilder();

            if (!bool.TryParse(useDongleString, out var useDongle))
            {
                stringBuilder.AppendLine("Please set the property 'NuixUseDongle' in the settings file");
            }

            if (string.IsNullOrWhiteSpace(nuixExeConsolePath))
            {
                stringBuilder.AppendLine("Please set the property 'NuixExeConsolePath' in the settings file");
            }

            if (!Version.TryParse(nuixVersionString, out var nuixVersion))
            {
                stringBuilder.AppendLine("Please set the property 'NuixVersion' in the settings file to a valid version number");
            }

            if (!TryParseNuixFeatures(nuixFeaturesString, out var nuixFeatures))
            {
                stringBuilder.AppendLine("Please set the property 'NuixFeatures' in the settings file to a comma separated list of nuix features or 'NO_FEATURES'");
            }

            var errorString = stringBuilder.ToString();

            if (!string.IsNullOrWhiteSpace(errorString))
                return Result.Failure<NuixSettings>(errorString);

#pragma warning disable CS8604 // Possible null reference argument. - this is checked above
            return new NuixSettings(useDongle, nuixExeConsolePath, nuixVersion, nuixFeatures);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        private static bool TryParseNuixFeatures(string? s, out IReadOnlyCollection<NuixFeature> nuixFeatures)
        {
            if(string.IsNullOrWhiteSpace(s))
            {
                nuixFeatures = new List<NuixFeature>();
                return false;
            }
            else if (s == "NO_FEATURES")
            {
                nuixFeatures = new List<NuixFeature>();
                return true;
            }
            else
            {
                var nfs = new HashSet<NuixFeature>();
                var features = s.Split(',');
                foreach (var feature in features)
                    if (Enum.TryParse(typeof(NuixFeature), feature, true, out var nf) && nf is NuixFeature nuixFeature)
                        nfs.Add(nuixFeature);

                nuixFeatures = nfs;
                return true;
            }
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


        private static readonly Regex NuixFeatureRegex =
            new Regex(@$"\A{RubyScriptStepUnit.NuixRequirementName}(?<feature>.+)\Z", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        public Result<Unit, IRunErrors> CheckRequirement(string processName, Requirement requirement)
        {
            if (requirement.Name == RubyScriptStepUnit.NuixRequirementName)
            {
                if(requirement.MinVersion != null && requirement.MinVersion > NuixVersion)
                    return new RunError($"Required Nuix Version >= {requirement.MinVersion} but had {NuixVersion}", processName, null, ErrorCode.RequirementsNotMet);

                if(requirement.MaxVersion != null && requirement.MaxVersion < NuixVersion)
                    return new RunError($"Required Nuix Version <= {requirement.MaxVersion} but had {NuixVersion}", processName, null, ErrorCode.RequirementsNotMet);

                return Unit.Default;
            }

            if (!NuixFeatureRegex.TryMatch(requirement.Name, out var match))
                return EmptySettings.Instance.CheckRequirement(processName, requirement);

            var feature = match.Groups["feature"].Value;

            if (Enum.TryParse<NuixFeature>(feature, true, out var nuixFeature) && NuixFeatures.Contains(nuixFeature))
                return Unit.Default;

            return new RunError($"{feature} missing", processName, null, ErrorCode.RequirementsNotMet);

        }
    }


}
