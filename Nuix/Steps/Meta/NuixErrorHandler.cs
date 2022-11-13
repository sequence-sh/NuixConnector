using System.Linq;
using Sequence.Core.ExternalProcesses;

namespace Sequence.Connectors.Nuix.Steps.Meta;

/// <summary>
/// Error Handler for running Nuix.exe
/// </summary>
public class NuixErrorHandler : IErrorHandler
{
    /// <summary>
    /// Create a new NuixErrorHandler.
    /// </summary>
    private NuixErrorHandler() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static IErrorHandler Instance = new NuixErrorHandler();

    /// <inheritdoc />
    public bool ShouldIgnoreError(string s)
    {
        if (WarningsToIgnore.Any(s.Contains))
            return true;

        return false;
    }

    private static readonly ISet<string> WarningsToIgnore =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ERROR StatusLogger Log4j2 could not find a logging implementation. Please add log4j-core to the classpath. Using SimpleLogger to log to the console...",
            "warning: ambiguous Java methods found, using open(java.lang.String)",
            "warning: multiple Java methods found, use -Xjruby.ji.ambiguous.calls.debug for backtrace. Choosing open(java.lang.String)"
        };
}
