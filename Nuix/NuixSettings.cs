using System;
using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix
{

/// <summary>
/// Methods to create Nuix Settings
/// </summary>
public static class NuixSettings
{
    /// <summary>
    /// Create Nuix Settings
    /// </summary>
    public static SCLSettings CreateSettings(
        string consolePath,
        Version version,
        IReadOnlyCollection<string> arguments,
        IReadOnlyCollection<NuixFeature> features)
    {
        var dict = new Dictionary<string, object>
        {
            {
                "nuix",
                new Dictionary<string, object>
                {
                    { "exeConsolePath", consolePath },
                    { "version", version.ToString() },
                    { "ConsoleArguments", arguments },
                    { "Features", features }
                }
            }
        };

        var entity = Entity.Create(("connectors", dict));

        return new SCLSettings(entity);
    }

    public static Version GetNuixVersion(SCLSettings settings)
    {
        var versionString =
            settings.Entity.TryGetValue("connectors")
                .Value.AsT7.TryGetValue("nuix")
                .Value.AsT7.TryGetValue("version")
                .Value.ToString()!;

        return Version.Parse(versionString);
    }

    /// <summary>
    /// Arguments to use for Dongle License source
    /// </summary>
    public static readonly IReadOnlyList<string> DongleArguments =
        new List<string> { "-licencesourcetype", "dongle" };
}

}
