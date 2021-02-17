using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Entities;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed record NuixConnectionSettings(Regex JavaWarningRegex, Regex JavaErrorRegex)
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <summary>
    /// Create a new NuixConnectionSettings
    /// </summary>
    public NuixConnectionSettings(string? javaWarningRegex, string? javaErrorRegex) : this(
        javaWarningRegex is null
            ? DefaultJavaWarningRegex
            : new Regex(javaWarningRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled),
        javaErrorRegex is null
            ? DefaultJavaErrorRegex
            : new Regex(javaErrorRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled)
    ) { }

    /// <summary>
    /// Create NuixConnectionSettings from SCL Settings
    /// </summary>
    public static NuixConnectionSettings Create(SCLSettings sclSettings)
    {
        var javaWarningsRegex = sclSettings.Entity.TryGetNestedString(
            SCLSettings.ConnectorsKey,
            NuixSettings.NuixSettingsKey,
            NuixSettings.IgnoreWarningsRegexKey
        )!.Unwrap();

        var javaErrorsRegex = sclSettings.Entity.TryGetNestedString(
            SCLSettings.ConnectorsKey,
            NuixSettings.NuixSettingsKey,
            NuixSettings.IgnoreErrorsRegexKey
        )!.Unwrap();

        return new NuixConnectionSettings(javaWarningsRegex, javaErrorsRegex);
    }

    private static readonly Regex DefaultJavaWarningRegex = new(
        @"\A(?:(?:\(eval\):9: warning)|(?:OpenJDK 64-Bit Server VM warning)|(?:WARNING)):(?<text>.+)\Z",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private static readonly Regex DefaultJavaErrorRegex = new(@"\AERROR\s*(?<text>.+)\Z");

    /// <summary>
    /// Default settings
    /// </summary>
    public static NuixConnectionSettings Default { get; } =
        new(DefaultJavaWarningRegex, DefaultJavaErrorRegex);
}

}
