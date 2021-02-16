using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Entities;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix
{

/// <summary>
/// Methods to create Nuix Settings
/// </summary>
public static class NuixSettings
{
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

    public const string SignoutKey = "signout";
    public const string ReleaseKey = "release";
    public const string LicenceSourceTypeKey = "licencesourcetype";
    public const string LicenceSourceLocationKey = "licencesourcelocation";
    public const string LicenceTypeKey = "licencetype";

    public const string LicenceWorkersKey = "licenceworkers";

    public const string NuixUsernameKey = "NUIX_USERNAME";

    public const string NuixPasswordKey = "NUIX_PASSWORD";
    // ReSharper restore StringLiteralTypo

    /// <summary>
    /// Create Nuix Settings
    /// </summary>
    public static SCLSettings CreateSettings(
        string consolePath,
        Version version,
        bool useDongle,
        IReadOnlyCollection<NuixFeature> features)
    {
        var dict = new Dictionary<string, object>
        {
            {
                NuixSettingsKey,
                new Dictionary<string, object>
                {
                    { ConsolePathKey, consolePath },
                    { SCLSettings.VersionKey, version.ToString() },
                    { LicenceSourceTypeKey, "dongle" },
                    { SCLSettings.FeaturesKey, features.Select(x => x.ToString()).ToList() }
                }
            }
        };

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
