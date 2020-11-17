using Newtonsoft.Json;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects
{
    /// <summary>
    /// The object if this was an error.
    /// </summary>
    public class ConnectionOutputError
    {
        /// <summary>
        /// The error message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; } = null!;
    }
}