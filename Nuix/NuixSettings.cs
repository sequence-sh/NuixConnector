using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix
{

/// <summary>
/// Methods to create Nuix Settings
/// </summary>
public static class NuixSettings
{
    /// <summary>
    /// The key to use for the Nuix Connector in the settings file.
    /// </summary>
    // ReSharper disable StringLiteralTypo
    public const string NuixSettingsKey = "Nuix";

    /// <summary>
    /// Console arguments that come before other parameters
    /// </summary>
    public const string ConsoleArgumentsKey = "ConsoleArguments";

    /// <summary>
    /// Console arguments that come after other parameters
    /// </summary>
    public const string ConsoleArgumentsPostKey = "ConsoleArgumentsPost";

    /// <summary>
    /// Environment variables to send to the Nuix Console
    /// </summary>
    public const string EnvironmentVariablesKey = "EnvironmentVariables";

    /// <summary>
    /// The path to the Nuix Console application
    /// </summary>
    public const string ConsolePathKey = "exeConsolePath";

    /// <summary>
    /// The path to the nuix ruby script
    /// </summary>
    public const string ScriptPathKey = "scriptPath";

    /// <summary>
    /// Signs the user out at the end of the execution, also releasing the semi-offline licence if present.
    /// </summary>
    public const string SignoutKey = "signout";

    /// <summary>
    /// Releases the semi-offline licence at the end of the execution.
    /// </summary>
    public const string ReleaseKey = "release";

    /// <summary>
    /// Selects a licence source type (e.g. dongle, server, cloud-server) to use. 
    /// </summary>
    public const string LicenceSourceTypeKey = "licencesourcetype";

    /// <summary>
    /// Selects a licence source if multiple are available. 
    /// </summary>
    public const string LicenceSourceLocationKey = "licencesourcelocation";

    /// <summary>
    /// Selects a licence type to use if multiple are available.
    /// </summary>
    public const string LicenceTypeKey = "licencetype";

    /// <summary>
    /// Selects the number of workers to use if the choice is available.
    /// </summary>
    public const string LicenceWorkersKey = "licenceworkers";

    /// <summary>
    /// Nuix Username - will be passed as an environment variable
    /// </summary>
    public const string NuixUsernameKey = "NUIX_USERNAME";

    /// <summary>
    /// Nuix password - will be passed as an environment variable
    /// </summary>
    public const string NuixPasswordKey = "NUIX_PASSWORD";
    // ReSharper restore StringLiteralTypo

    /// <summary>
    /// Regex used to ignore java warnings coming from the Nuix connection.
    /// The default values ignores warnings from Nuix Version up to 9.
    /// </summary>
    public const string IgnoreWarningsRegexKey = "IgnoreWarningsRegex";

    /// <summary>
    /// Regex used to ignore java errors coming from the Nuix connection.
    /// The default values ignores errors from Nuix Version up to 9.
    /// </summary>
    public const string IgnoreErrorsRegexKey = "IgnoreErrorsRegex";

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

    /// <summary>
    /// Create Nuix Settings
    /// </summary>
    public static SCLSettings CreateSettings(
        string consolePath,
        Version version,
        bool useDongle,
        IReadOnlyCollection<NuixFeature> features)
    {
        var settingsDict = new Dictionary<string, object>
        {
            { ConsolePathKey, consolePath },
            { SCLSettings.VersionKey, version.ToString() },
            { SCLSettings.FeaturesKey, features.Select(x => x.ToString()).ToList() }
        };

        if (useDongle)
            settingsDict.Add(LicenceSourceTypeKey, "dongle");

        var dict = new Dictionary<string, object> { { NuixSettingsKey, settingsDict } };

        var entity = Entity.Create((SCLSettings.ConnectorsKey, dict));

        return new SCLSettings(entity);
    }

    /// <summary>
    /// Tries to get the nuix version from a settings object
    /// </summary>
    public static Maybe<Version> TryGetNuixVersion(SCLSettings settings)
    {
        var versionString = settings.Entity.TryGetNestedString(
            SCLSettings.ConnectorsKey,
            NuixSettingsKey,
            SCLSettings.VersionKey
        );

        if (versionString.HasNoValue)
            return Maybe<Version>.None;

        if (Version.TryParse(versionString.Value, out var v))
            return v;

        return Maybe<Version>.None;
    }
}

}
