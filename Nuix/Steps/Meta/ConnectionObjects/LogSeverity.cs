using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects
{

/// <summary>
/// The severity of a log message.
/// </summary>
public enum LogSeverity
{
    /// <summary>
    /// Trace
    /// </summary>
    [JsonProperty("trace")]
    Trace,

    /// <summary>
    /// Information
    /// </summary>
    [JsonProperty("info")]
    Information,

    /// <summary>
    /// Warning
    /// </summary>
    [JsonProperty("warn")]
    Warning,

    /// <summary>
    /// Error
    /// </summary>
    [JsonProperty("error")]
    Error,

    /// <summary>
    /// Critical
    /// </summary>
    [JsonProperty("fatal")]
    Critical,

    /// <summary>
    /// Debug
    /// </summary>
    [JsonProperty("debug")]
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

}
