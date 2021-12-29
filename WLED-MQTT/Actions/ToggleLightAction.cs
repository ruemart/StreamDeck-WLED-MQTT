using BarRaider.SdTools;
using System;
using WLED_MQTT.Mqtt;

namespace WLED_MQTT.Actions
{
    [PluginActionId("com.ruemart.wledmqtt.togglelight")]
    public sealed class ToggleLightAction : BaseMqttAction<ToggleLightAction.PluginSettings>
    {
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
                };
                return instance;
            }
        }

        public ToggleLightAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
                    await mqttClient.SendAsync("Test/Toggle", "HelloWorld");
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
            mqttClient.SendAsync("Test/Toggle", "HelloWorld!!!");
        }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload) { }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }
    }
}
