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

}
