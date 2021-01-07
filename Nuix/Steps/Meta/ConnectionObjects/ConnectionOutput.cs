using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects
{

/// <summary>
/// An output object that will be returned from a Nuix Connection
/// </summary>
public class ConnectionOutput
{
    /// <summary>
    /// The object if this is the result of a function.
    /// </summary>
    [JsonProperty("result")]
    public ConnectionOutputResult? Result { get; set; }

    /// <summary>
    /// The object if this is a log message.
    /// </summary>
    [JsonProperty("log")]
    public ConnectionOutputLog? Log { get; set; }

    /// <summary>
    /// The object if this is an error message.
    /// </summary>
    [JsonProperty("error")]
    public ConnectionOutputError? Error { get; set; }

    /// <summary>
    /// A function to ensure that only one of Result, Log, or Error are set
    /// </summary>
    /// <returns>true if ConnectionOutput is valid, ErrorBuilder if not</returns>
    public Result<bool, IErrorBuilder> Validate()
    {
        var count = 0;

        if (Result != null)
            count++;

        if (Log != null)
            count++;

        if (Error != null)
            count++;

        if (count == 1)
            return true;

        if (count > 1)
            return new ErrorBuilder(
                $"{nameof(ConnectionOutput)} can only have one property set",
                ErrorCode.ExternalProcessError
            );

        return new ErrorBuilder(
            $"{nameof(ConnectionOutput)} must have at least one property set",
            ErrorCode.ExternalProcessMissingOutput
        );
    }
}

}
