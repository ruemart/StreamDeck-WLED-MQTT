using System;

namespace WLED_MQTT.Mqtt
{
    /// <summary>
    /// Event handler that is used if the status of <see cref="MqttClient"/> changed.
    /// </summary>
    public class MqttClientStatusChangedEventHandler : EventArgs
    {
        /// <summary>
        /// Gets or sets the new status of the <see cref="MqttClient"/>.
        /// </summary>
        public MqttStatus NewStatus { get; set; }
    }
}
