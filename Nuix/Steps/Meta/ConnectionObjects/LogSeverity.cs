using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;

/// <summary>
/// The severity of a log message.
/// </summary>
public enum LogSeverity
{
    /// <summary>
    /// Trace
    /// </summary>
    [JsonPropertyName("trace")]
    Trace,

    /// <summary>
    /// Information
    /// </summary>
    [JsonPropertyName("info")]
    Information,

    /// <summary>
    /// Warning
    /// </summary>
    [JsonPropertyName("warn")]
    Warning,

    /// <summary>
    /// Error
    /// </summary>
    [JsonPropertyName("error")]
    Error,

    /// <summary>
    /// Critical
    /// </summary>
    [JsonPropertyName("fatal")]
    Critical,

    /// <summary>
    /// Debug
    /// </summary>
    [JsonPropertyName("debug")]
    Debug
}

/// <summary>
/// Contains methods to help with LogSeverities
/// </summary>
public static class LogSeverityHelper
{
    /// <summary>
    /// Convert a LogSeverity to a LogLevel
    /// </summary>
    public static LogLevel ToLogLevel(this LogSeverity ls)
    {
        var logLevel = ls switch
        {
            LogSeverity.Trace       => LogLevel.Trace,
            LogSeverity.Information => LogLevel.Information,
            LogSeverity.Warning     => LogLevel.Warning,
            LogSeverity.Error       => LogLevel.Error,
            LogSeverity.Critical    => LogLevel.Critical,
            LogSeverity.Debug       => LogLevel.Debug,
            _                       => throw new ArgumentOutOfRangeException(nameof(ls), ls, null)
        };

        return logLevel;
    }
}
