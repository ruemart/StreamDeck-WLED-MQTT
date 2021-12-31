using BarRaider.SdTools;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
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

        private MqttStatus currentMqttStatus;

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

        protected override void OnMqttClientStatusChanged(object sender, MqttClientStatusChangedEventHandler e)
        {
            currentMqttStatus = e.NewStatus;
            switch (currentMqttStatus)
            {
                case MqttStatus.NotRunning:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ToggleLight: MQTT Offline");
                    break;
                case MqttStatus.Connecting:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ToggleLight: MQTT Connecting");
                    break;
                case MqttStatus.Faulty:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ToggleLight: MQTT Failed");
                    break;
                case MqttStatus.Running:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ToggleLight: MQTT Running");
                    break;
            }
            ChangeKeyImage(currentMqttStatus);
        }

        protected override async void ChangeKeyImage(MqttStatus status)
        {
            using (var bmp = Tools.GenerateGenericKeyImage(out var graphics))
            {
                Image background = null;
                switch (status)
                {
                    case MqttStatus.NotRunning:
                        background = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "Images", "ToggleOffline@2x.png"));
                        break;
                    case MqttStatus.Connecting:
                        background = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "Images", "ToggleConnecting@2x.png"));
                        break;
                    case MqttStatus.Faulty:
                        background = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "Images", "ToggleError@2x.png"));
                        break;
                    case MqttStatus.Running:
                        background = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "Images", "ToggleOnline@2x.png"));
                        break;
                }
                graphics.DrawImage(background, 0, 0, bmp.Width, bmp.Height);

                await Connection.SetImageAsync(bmp);
                graphics.Dispose();
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