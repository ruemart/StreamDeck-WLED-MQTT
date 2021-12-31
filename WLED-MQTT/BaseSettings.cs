using Newtonsoft.Json;

namespace WLED_MQTT
{
    public class BaseSettings
    {
        /// <summary>
        /// Gets or sets the id of the client.
        /// </summary>
        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; } = string.Empty;
    }
}
