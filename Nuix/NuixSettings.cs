using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix
{

/// <summary>
/// Special settings, unique to the Nuix connector
/// </summary>
[Serializable]
public class NuixSettings : IEntityConvertible
{
    /// <summary>
    /// Empty constructor for deserialization
    /// </summary>
    public NuixSettings() { }

    /// <summary>
    /// Create nuix settings with default settings
    /// </summary>
    public NuixSettings(
        string nuixConsolePath,
        Version version,
        bool useDongle,
        IEnumerable<NuixFeature> nuixFeatures)
    {
        ExeConsolePath = nuixConsolePath;
        Version        = version;
        Features       = nuixFeatures.ToList();

        if (useDongle)
            LicenceSourceType = "dongle";
        //Constants.NuixConsoleExe,
        //new Version(8, 0),
        //true,
        //Constants.AllNuixFeatures
    }

    /// <summary>
    /// Console arguments that come before other parameters
    /// </summary>
    [JsonProperty("ConsoleArguments")]
    public List<string>? ConsoleArguments { get; set; }

    /// <summary>
    /// Console arguments that come after other parameters
    /// </summary>
    [JsonProperty("ConsoleArgumentsPost")]
    public List<string>? ConsoleArgumentsPost { get; set; }

    [JsonProperty("features")] public List<NuixFeature>? Features { get; set; }

    /// <summary>
    /// Environment variables to send to the Nuix Console
    /// </summary>
    [JsonProperty("EnvironmentVariables")]
    public Entity? EnvironmentVariables { get; set; }

    /// <summary>
    /// The version of the nuix console application
    /// </summary>
    [JsonProperty("Version")]
    public Version? Version { get; set; }

    /// <summary>
    /// The path to the Nuix Console application
    /// </summary>
    [JsonProperty("exeConsolePath")]
    public string ExeConsolePath { get; set; }

    /// <summary>
    /// The path to the nuix ruby script
    /// </summary>
    [JsonProperty("scriptPath")]
    public string? ScriptPath { get; set; }

    /// <summary>
    /// Signs the user out at the end of the execution, also releasing the semi-offline licence if present.
    /// </summary>
    [JsonProperty("signout")]
    public bool Signout { get; set; }

    /// <summary>
    /// Releases the semi-offline licence at the end of the execution.
    /// </summary>
    [JsonProperty("release")]
    public bool Release { get; set; }

    /// <summary>
    /// Selects a licence source type (e.g. dongle, server, cloud-server) to use. 
    /// </summary>
    [JsonProperty("licencesourcetype")]
    public string? LicenceSourceType { get; set; }

    /// <summary>
    /// Selects a licence source if multiple are available. 
    /// </summary>
    [JsonProperty("licencesourcelocation")]
    public string? LicenseSourceLocation { get; set; }

    /// <summary>
    /// Selects a licence type to use if multiple are available.
    /// </summary>
    [JsonProperty("licencetype")]
    public string? LicenceType { get; set; }

    /// <summary>
    /// Selects the number of workers to use if the choice is available.
    /// </summary>
    [JsonProperty("licenceworkers")]
    public int? LicenceWorkers { get; set; }

    /// <summary>
    /// Nuix Username - will be passed as an environment variable
    /// </summary>
    [JsonProperty("NUIX_USERNAME")]
    public string? NuixUsername { get; set; }

    /// <summary>
    /// Nuix password - will be passed as an environment variable
    /// </summary>
    [JsonProperty("NUIX_PASSWORD")]
    public string? NuixPassword { get; set; }

    /// <summary>
    /// Regex used to ignore java warnings coming from the Nuix connection.
    /// The default values ignores warnings from Nuix Version up to 9.
    /// </summary>
    [JsonProperty("IgnoreWarningsRegex")]
    public string? IgnoreWarningsRegex { get; set; }

    /// <summary>
    /// Regex used to ignore java errors coming from the Nuix connection.
    /// The default values ignores errors from Nuix Version up to 9.
    /// </summary>
    [JsonProperty("IgnoreErrorsRegex")]
    public string IgnoreErrorsRegex { get; set; }
}

/// <summary>
/// Contains helper methods for nuix settings
/// </summary>
public static class SettingsHelpers
{
    /// <summary>
    /// Create Nuix Settings
    /// </summary>
    public static SCLSettings CreateSCLSettings(NuixSettings nuixSettings)
    {
        var connectorSettings =
            ConnectorSettings.DefaultForAssembly(Assembly.GetAssembly(typeof(IRubyScriptStep))!);

        connectorSettings.Settings = nuixSettings.ConvertToEntity();

        var dict =
            new Dictionary<string, object> { { "nuix", connectorSettings.ConvertToEntity() } };

        var entity = Entity.Create((SCLSettings.ConnectorsKey, dict));

        return new SCLSettings(entity);
    }

    /// <summary>
    /// Try to get a list of NuixSettings from a list of Connector Informations
    /// </summary>
    public static Result<NuixSettings, IErrorBuilder> TryGetNuixSettings(
        IEnumerable<ConnectorSettings> connectorSettings)
    {
        var connectorInformation =
            connectorSettings.FirstOrDefault(
                x => x.Id.EndsWith("Nuix", StringComparison.OrdinalIgnoreCase)
            );

        if (connectorInformation is null)
            return ErrorCode.MissingStepSettings.ToErrorBuilder("Nuix");

        var nuixSettings =
            EntityConversionHelpers.TryCreateFromEntity<NuixSettings>(
                connectorInformation.Settings
            );

        return nuixSettings;
    }

    /// <summary>
    /// Returns a new SCLSettings object with a property added
    /// </summary>
    public static SCLSettings WithProperty(
        this SCLSettings settings,
        object? value,
        params string[] pathComponents)
    {
        var e = Entity.Create(new[] { (new EntityPropertyKey(pathComponents), value) });

        return new SCLSettings(settings.Entity.Combine(e));
    }
}

}
