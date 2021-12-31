using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Threading.Tasks;
using WLED_MQTT.Mqtt;

namespace WLED_MQTT.Actions
{
    [PluginActionId("com.ruemart.wledmqtt.changefx")]
    public sealed class ChangeFxAction : BaseMqttAction<ChangeFxAction.PluginSettings>
    {
        public class PluginSettings : BaseSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    Effects = WledEffectList.Create(),
                    SelectedEffect = 0,
                    ClientId = Guid.NewGuid().ToString(),
                };
                return instance;
            }

            [JsonProperty(PropertyName = "effects")]
            public List<WledEffect> Effects { get; set; }

            [JsonProperty(PropertyName = "selectedEffect")]
            public int SelectedEffect { get; set; }
        }

        public ChangeFxAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                settings = PluginSettings.CreateDefaultSettings();
            }
            else
            {
                settings = payload.Settings.ToObject<PluginSettings>();
            }

            settings.Effects = WledEffectList.Create();
            SaveSettings();
            Connection.GetGlobalSettingsAsync();
        }

        protected override async void OnMqttClientStatusChanged(object sender, MqttClientStatusChangedEventHandler e)
        {
            switch (e.NewStatus)
            {
                case MqttStatus.NotRunning:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ChangeFX: MQTT Offline");
                    Console.WriteLine($"ChangeFX: MQTT Offline");
                    break;
                case MqttStatus.Connecting:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ChangeFX: MQTT Connecting");
                    Console.WriteLine($"ChangeFX: MQTT Connecting");
                    break;
                case MqttStatus.Faulty:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ChangeFX: MQTT Failed");
                    Console.WriteLine($"ChangeFX: MQTT Failed");
                    break;
                case MqttStatus.Running:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ChangeFX: MQTT Running");
                    Console.WriteLine($"ChangeFX: MQTT Running");
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
            mqttClient.SendAsync(globalSettings.DeviceTopic + "/api", $"FX={settings.SelectedEffect}");
        }

        public override void OnTick() { }

        public override async void ReceivedSettings(ReceivedSettingsPayload payload) 
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            await SaveSettings();
            Tools.AutoPopulateSettings(globalSettings, payload.Settings);
            await SaveGlobalSettings();
        }

        protected override Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }
    }
}
