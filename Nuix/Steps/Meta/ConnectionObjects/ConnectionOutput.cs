using Newtonsoft.Json;

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
    }
}
