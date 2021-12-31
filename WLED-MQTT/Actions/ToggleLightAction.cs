﻿using BarRaider.SdTools;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using WLED_MQTT.Mqtt;

namespace WLED_MQTT.Actions
{
    [PluginActionId("com.ruemart.wledmqtt.togglelight")]
    public sealed class ToggleLightAction : BaseMqttAction<ToggleLightAction.PluginSettings>
    {
        public class PluginSettings : BaseSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    ClientId = Guid.NewGuid().ToString(),
                };
                return instance;
            }
        }

        public ToggleLightAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                settings = PluginSettings.CreateDefaultSettings();
            }
            else
            {
                settings = payload.Settings.ToObject<PluginSettings>();
            }

            SaveSettings();
            Connection.GetGlobalSettingsAsync();
        }

        protected override async void OnMqttClientStatusChanged(object sender, MqttClientStatusChangedEventHandler e)
        {
            switch (e.NewStatus)
            {
                case MqttStatus.NotRunning:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ToggleLight: MQTT Offline");
                    Console.WriteLine($"ToggleLight: MQTT Offline");
                    break;
                case MqttStatus.Connecting:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ToggleLight: MQTT Connecting");
                    Console.WriteLine($"ToggleLight: MQTT Connecting");
                    break;
                case MqttStatus.Faulty:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ToggleLight: MQTT Failed");
                    Console.WriteLine($"ToggleLight: MQTT Failed");
                    break;
                case MqttStatus.Running:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ToggleLight: MQTT Running");
                    Console.WriteLine($"ToggleLight: MQTT Running");
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
            mqttClient.SendAsync(globalSettings.DeviceTopic, "T");
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