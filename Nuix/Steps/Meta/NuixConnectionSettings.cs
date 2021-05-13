using System.Text.RegularExpressions;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// Settings pertaining to the Nuix Connection
/// </summary>
public sealed record NuixConnectionSettings(Regex JavaWarningRegex, Regex JavaErrorRegex)
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
