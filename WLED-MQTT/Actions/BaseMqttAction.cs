using BarRaider.SdTools;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using WLED_MQTT.Mqtt;

namespace WLED_MQTT.Actions
{
    public abstract class BaseMqttAction<TSettings> : PluginBase where TSettings : BaseSettings
    {
        protected MqttClient mqttClient;
        protected GlobalPluginSettings globalSettings;
        protected TSettings settings;

        public BaseMqttAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Destructor called");
            mqttClient.StatusChanged -= OnMqttClientStatusChanged;
            mqttClient.Dispose();
        }

        public override async void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) 
        {
            if (payload?.Settings != null && payload.Settings.Count > 0)
            {
                globalSettings = payload.Settings.ToObject<GlobalPluginSettings>();
            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"No global settings found, creating new object");
                globalSettings = new GlobalPluginSettings();
                await SaveGlobalSettings();
            }
            await SetDefaultGlobalValues();

            if(mqttClient == null)
            {
                if(string.IsNullOrEmpty(settings.ClientId))
                {
                    settings.ClientId = Guid.NewGuid().ToString();
                    await SaveSettings();
                }

                await SetupMqtt();
            }
            else
            {
                await mqttClient.StopMqttClientAsync();
            }

            await mqttClient.StartMqttClientAsync(globalSettings, settings.ClientId);
        }

        protected async Task SetDefaultGlobalValues()
        {
            var needsToSave = false;
            if (string.IsNullOrWhiteSpace(globalSettings.Host))
            {
                globalSettings.Host = "localhost";
                needsToSave = true;
            }

            if (globalSettings.CommunicationTimeout < 0)
            {
                globalSettings.CommunicationTimeout = 0;
                needsToSave = true;
            }

            if (globalSettings.Port < 1025 || globalSettings.Port > 65535)
            {
                globalSettings.Port = 1883;
                needsToSave = true;
            }

            if(needsToSave)
            {
                await SaveGlobalSettings();
            }
        }


        protected Task SaveGlobalSettings()
        {
            return Connection.SetGlobalSettingsAsync(JObject.FromObject(globalSettings));
        }

        protected virtual void OnMqttClientStatusChanged(object sender, MqttClientStatusChangedEventHandler e) { }

        protected abstract Task SaveSettings();

        protected abstract void ChangeKeyImage(MqttStatus status);
        private async Task SetupMqtt()
        {
            mqttClient = new MqttClient();
            mqttClient.StatusChanged += OnMqttClientStatusChanged;
            await mqttClient.StartMqttClientAsync(globalSettings, settings.ClientId);
        }

    }
}
