using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
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
        bool useDongle,
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
                    { "licencesourcetype", "dongle" },
                    { "Features", features.Select(x => x.ToString()).ToList() }
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
    /// Tries to get a nested string.
    /// </summary>
    public static Maybe<string>
        TryGetNestedString(
            this Entity current,
            params string[] properties) //TODO remove when we can update core
    {
        if (!properties.Any())
            return Maybe<string>.None;

        foreach (var property in properties.SkipLast(1))
        {
            var v = current.TryGetValue(property);

            if (v.HasNoValue)
                return Maybe<string>.None;

            if (v.Value.TryPickT7(out var e, out _))
                current = e;
            else
                return Maybe<string>.None;
        }

        var lastProp = current.TryGetValue(properties.Last());

        if (lastProp.HasNoValue)
            return Maybe<string>.None;

        return lastProp.Value.ToString();
    }

    public static bool TryGetNestedBool(this Entity current, params string[] properties)
    {
        var s = TryGetNestedString(current, properties);

        if (s.HasNoValue)
            return false;

        var b = bool.TryParse(s.Value, out var r) && r;

        return b;
    }

    /// <summary>
    /// Tries to get a nested string.
    /// </summary>
    public static Maybe<string[]>
        TryGetNestedList(
            this Entity current,
            params string[] properties) //TODO remove when we can update core
    {
        if (!properties.Any())
            return Maybe<string[]>.None;

        foreach (var property in properties.SkipLast(1))
        {
            var v = current.TryGetValue(property);

            if (v.HasNoValue)
                return Maybe<string[]>.None;

            if (v.Value.TryPickT7(out var e, out _))
                current = e;
            else
                return Maybe<string[]>.None;
        }

        var lastProp = current.TryGetValue(properties.Last());

        if (lastProp.HasNoValue)
            return Maybe<string[]>.None;

        if (!lastProp.Value.TryPickT8(out var list, out _))
            return Maybe<string[]>.None;

        var stringArray = list.Select(x => x.ToString()).ToArray();
        return stringArray;
    }
}

}
