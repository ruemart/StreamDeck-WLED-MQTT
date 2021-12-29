using BarRaider.SdTools;
using Newtonsoft.Json;
using WLED_MQTT.Mqtt;

namespace WLED_MQTT
{
    public class BasePluginSettings
    {
        /// <summary>
        /// Gets or sets the connection type to use for the MQTT client.
        /// </summary>
        [FilenameProperty]
        [JsonProperty(PropertyName = "connectionType")]
        public ConnectionType ConnectionType { get; set; } = ConnectionType.TCP;

        /// <summary>
        /// Gets or sets the port of the MQTT broker.
        /// </summary>
        [FilenameProperty]
        [JsonProperty(PropertyName = "port")]
        public int Port { get; set; } = 1883;

        /// <summary>
        /// Gets or sets the host adress of the MQTT broker.
        /// </summary>
        [FilenameProperty]
        [JsonProperty(PropertyName = "host")]
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets a path to the certificate to use when connecting to the MQTT broker.
        /// </summary>
        [FilenameProperty]
        [JsonProperty(PropertyName = "certificateFile")]
        public string CertificateFile { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the adress of the MQTT broker.
        /// </summary>
        [FilenameProperty]
        [JsonProperty(PropertyName = "webSocketServerAdress")]
        public string WebSocketServerAdress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the id of the client.
        /// </summary>
        [FilenameProperty]
        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user for login.
        /// </summary>
        [FilenameProperty]
        [JsonProperty(PropertyName = "user")]
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for login.
        /// </summary>
        [FilenameProperty]
        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the time out for MQTT communication.
        /// </summary>
        [FilenameProperty]
        [JsonProperty(PropertyName = "communicationTimeout")]
        public int CommunicationTimeout { get; set; } = 0;
    }
}
