﻿using BarRaider.SdTools;
using Newtonsoft.Json;
using System;
using WLED_MQTT.Mqtt;

namespace WLED_MQTT.Actions
{
    [PluginActionId("com.ruemart.wledmqtt.changecolor")]
    public sealed class ChangeColorAction : BaseMqttAction<ChangeColorAction.PluginSettings>
    {
        public enum Mode
        {
            Manual = 0,

            Automatic = 1,
        }

        public class PluginSettings : BasePluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    ConnectionType = ConnectionType.TCP,
                    Port = 1883,
                    Host = "localhost",
                    CertificateFile = string.Empty,
                    WebSocketServerAdress = string.Empty,
                    ClientId = Guid.NewGuid().ToString(),
                    User = string.Empty,
                    Password = string.Empty,
                    Color = "#FF0000",
                    Mode = Mode.Manual,
                    RotationSpeed = 60.ToString(),
                };
                return instance;
            }

            /// <summary>
            /// Gets or sets the color to set.
            /// </summary>
            [JsonProperty(PropertyName = "color")]
            public string Color { get; set; }

            /// <summary>
            /// Gets or sets whether the colors change automatically or via key press.
            /// </summary>
            [JsonProperty(PropertyName = "mode")]
            public Mode Mode { get; set; }

            /// <summary>
            /// Gets or sets the time between automatic color changes.
            /// </summary>
            [JsonProperty(PropertyName = "rotationSpeed")]
            public string RotationSpeed { get; set; }
        }

        public ChangeColorAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                settings = PluginSettings.CreateDefaultSettings();
                SaveSettings();
            }
            else
            {
                settings = payload.Settings.ToObject<PluginSettings>();
            }
        }

        protected override async void OnMqttClientStatusChanged(object sender, MqttClientStatusChangedEventHandler e)
        {
            switch (e.NewStatus)
            {
                case MqttStatus.NotRunning:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"MQTT Offline");
                    Console.WriteLine($"MQTT Offline");
                    break;
                case MqttStatus.Connecting:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"MQTT Connecting");
                    Console.WriteLine($"MQTT Connecting");
                    break;
                case MqttStatus.Faulty:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"MQTT Failed");
                    Console.WriteLine($"MQTT Failed");
                    break;
                case MqttStatus.Running:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"MQTT Running");
                    Console.WriteLine($"MQTT Running");
                    await mqttClient.SendAsync("Test/Color", "HelloWorld");
                    break;
            }
        }

        public override void Dispose() { }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
        }

        public override void KeyReleased(KeyPayload payload)
        {
            mqttClient.SendAsync("Test/Color", "HelloWorld!!!");
        }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload) { }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
    }
}