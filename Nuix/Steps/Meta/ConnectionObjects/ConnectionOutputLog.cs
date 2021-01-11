using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects
{

/// <summary>
/// A Log message from a Nuix connection.
/// </summary>
public class ConnectionOutputLog
{
    /// <summary>
    /// The severity of the log.
    /// </summary>
    [JsonProperty("severity")]
    public string Severity { get; set; } = null!;

    /// <summary>
    /// The log message.
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; } = null!;

    /// <summary>
    /// The time this message was logged.
    /// </summary>
    [JsonProperty("time")]
    public string Time { get; set; } = null!;

    /// <summary>
    /// Stack trace of the message.
    /// </summary>
    [JsonProperty("stackTrace")]
    public string StackTrace { get; set; } = null!;

    /// <summary>
    /// Get the severity as an object
    /// </summary>
    public Result<LogSeverity, IErrorBuilder> TryGetSeverity()
    {
        return Severity.ToLowerInvariant() switch
        {
            "trace" => LogSeverity.Trace,
            "info"  => LogSeverity.Information,
            "warn"  => LogSeverity.Warning,
            "error" => LogSeverity.Error,
            "fatal" => LogSeverity.Critical,
            "debug" => LogSeverity.Debug,
            _       => new ErrorBuilder(ErrorCode.CouldNotParse, Severity, nameof(LogSeverity)),
        };
    }
}

}
