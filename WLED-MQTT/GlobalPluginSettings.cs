using BarRaider.SdTools;
using Newtonsoft.Json;
using WLED_MQTT.Mqtt;

namespace WLED_MQTT
{
    public class GlobalPluginSettings
    {
        /// <summary>
        /// Gets or sets the connection type to use for the MQTT client.
        /// </summary>
        [JsonProperty(PropertyName = "connectionType")]
        public ConnectionType ConnectionType { get; set; } = ConnectionType.SecureTCP;

        /// <summary>
        /// Gets or sets the port of the MQTT broker.
        /// </summary>
        [JsonProperty(PropertyName = "port")]
        public int Port { get; set; } = 1883;

        /// <summary>
        /// Gets or sets the host adress of the MQTT broker.
        /// </summary>
        [JsonProperty(PropertyName = "host")]
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the host adress of the MQTT broker.
        /// </summary>
        [JsonProperty(PropertyName = "deviceTopic")]
        public string DeviceTopic { get; set; } = "wled/all";

        /// <summary>
        /// Gets or sets the adress of the MQTT broker.
        /// </summary>
        [JsonProperty(PropertyName = "webSocketServerAdress")]
        public string WebSocketServerAdress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user for login.
        /// </summary>
        [JsonProperty(PropertyName = "user")]
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for login.
        /// </summary>
        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the time out for MQTT communication.
        /// </summary>
        [JsonProperty(PropertyName = "communicationTimeout")]
        public int CommunicationTimeout { get; set; } = 0;
    }
}
