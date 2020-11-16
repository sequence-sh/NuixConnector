using Newtonsoft.Json;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects
{
    /// <summary>
    /// The object if this was a result.
    /// </summary>
    public class ConnectionOutputResult
    {
        /// <summary>
        /// The result of the function.
        /// Will be null if the function returns void.
        /// </summary>
        [JsonProperty("data")]
        public object? Data { get; set; }
    }
}