using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Internal.Logging;

namespace Reductech.EDR.Connectors.Nuix.Logging;

/// <summary>
/// Identifying code for a Core log situation.
/// </summary>
public sealed record LogSituationNuix : LogSituationBase
{
    private LogSituationNuix(string code, LogLevel logLevel) : base(code, logLevel) { }

    /// <inheritdoc />
    protected override string GetLocalizedString()
    {
        var localizedMessage = LogMessages_EN.ResourceManager.GetString(Code);

        Debug.Assert(localizedMessage != null, nameof(localizedMessage) + " != null");
        return localizedMessage;
    }

    /// <summary>
    /// Helper function {FunctionName} already exists.
    /// </summary>
    public static readonly LogSituationNuix HelperExists = new(
        nameof(HelperExists),
        LogLevel.Trace
    );
}
