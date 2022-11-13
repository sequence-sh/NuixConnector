using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using Sequence.ConnectorManagement.Base;
using Sequence.Core.Abstractions;
using Sequence.Core.Internal.Errors;
using Entity = Sequence.Core.Entity;

namespace Sequence.Connectors.Nuix;

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
    }

    /// <summary>
    /// Console arguments that come before other parameters
    /// </summary>
    [JsonPropertyName("ConsoleArguments")]
    public List<string>? ConsoleArguments { get; set; }

    /// <summary>
    /// Console arguments that come after other parameters
    /// </summary>
    [JsonPropertyName("ConsoleArgumentsPost")]
    public List<string>? ConsoleArgumentsPost { get; set; }

    /// <summary>
    /// List of available Nuix features
    /// </summary>
    [JsonPropertyName("features")]
    public List<NuixFeature>? Features { get; set; }

    /// <summary>
    /// Environment variables to send to the Nuix Console
    /// </summary>
    [JsonPropertyName("EnvironmentVariables")]
    public Entity? EnvironmentVariables { get; set; }

    /// <summary>
    /// The version of the nuix console application
    /// </summary>
    [JsonPropertyName("Version")]
    public Version? Version { get; set; }

    /// <summary>
    /// The path to the Nuix Console application
    /// </summary>
    [JsonPropertyName("exeConsolePath")]
    public string ExeConsolePath { get; set; }

    /// <summary>
    /// The path to the nuix ruby script
    /// </summary>
    [JsonPropertyName("scriptPath")]
    public string? ScriptPath { get; set; }

    /// <summary>
    /// Signs the user out at the end of the execution, also releasing the semi-offline licence if present.
    /// </summary>
    [JsonPropertyName("signout")]
    public bool Signout { get; set; }

    /// <summary>
    /// Releases the semi-offline licence at the end of the execution.
    /// </summary>
    [JsonPropertyName("release")]
    public bool Release { get; set; }

    /// <summary>
    /// Selects a licence source type (e.g. dongle, server, cloud-server) to use. 
    /// </summary>
    [JsonPropertyName("licencesourcetype")]
    public string? LicenceSourceType { get; set; }

    /// <summary>
    /// Selects a licence source if multiple are available. 
    /// </summary>
    [JsonPropertyName("licencesourcelocation")]
    public string? LicenseSourceLocation { get; set; }

    /// <summary>
    /// Selects a licence type to use if multiple are available.
    /// </summary>
    [JsonPropertyName("licencetype")]
    public string? LicenceType { get; set; }

    /// <summary>
    /// Selects the number of workers to use if the choice is available.
    /// </summary>
    [JsonPropertyName("licenceworkers")]
    public int? LicenceWorkers { get; set; }

    /// <summary>
    /// Nuix Username - will be passed as an environment variable
    /// </summary>
    [JsonPropertyName("NUIX_USERNAME")]
    public string? NuixUsername { get; set; }

    /// <summary>
    /// Nuix password - will be passed as an environment variable
    /// </summary>
    [JsonPropertyName("NUIX_PASSWORD")]
    public string? NuixPassword { get; set; }

    /// <summary>
    /// Regex used to ignore java warnings coming from the Nuix connection.
    /// The default values ignores warnings from Nuix Version up to 9.
    /// </summary>
    [JsonPropertyName("IgnoreWarningsRegex")]
    public string? IgnoreWarningsRegex { get; set; }

    /// <summary>
    /// Regex used to ignore java errors coming from the Nuix connection.
    /// The default values ignores errors from Nuix Version up to 9.
    /// </summary>
    [JsonPropertyName("IgnoreErrorsRegex")]
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
    public static StepFactoryStore CreateStepFactoryStore(
        NuixSettings? nuixSettings,
        params Assembly[] additionalAssemblies)
    {
        var nuix = Assembly.GetAssembly(typeof(IRubyScriptStep));

        var ns = ConnectorSettings.DefaultForAssembly(nuix!);

        ns.Settings = nuixSettings?.ConvertToEntity()
            .ToDictionary(
                k => k.Key.Inner,
                v => v.Value.ToCSharpObject(),
                StringComparer.OrdinalIgnoreCase
            )!;

        var core = Assembly.GetAssembly(typeof(IStep));

        var cd = new List<ConnectorData>
        {
            new(ConnectorSettings.DefaultForAssembly(core!), core), new(ns, nuix)
        };

        cd.AddRange(
            additionalAssemblies.Select(
                a => new ConnectorData(ConnectorSettings.DefaultForAssembly(a), a)
            )
        );

        var sfs = StepFactoryStore.TryCreate(ExternalContext.Default, cd.ToArray())
            .Value; //TODO inject the external context

        return sfs;
    }

    /// <summary>
    /// Try to get a list of NuixSettings from the global settings Entity
    /// </summary>
    public static Result<NuixSettings, IErrorBuilder> TryGetNuixSettings(Entity settings)
    {
        var nuixKey = "Sequence.Connectors.Nuix";

        var nuixConnector = settings.TryGetValue(
            new EntityNestedKey(
                StateMonad.ConnectorsKey,
                nuixKey,
                nameof(ConnectorData.ConnectorSettings.Settings)
            )
        );

        if (nuixConnector.HasNoValue
         || nuixConnector.GetValueOrThrow() is not Entity ent)
            return ErrorCode.MissingStepSettings.ToErrorBuilder(nuixKey);

        var settingsObj =
            EntityConversionHelpers.TryCreateFromEntity<NuixSettings>(ent);

        return settingsObj;
    }
}
