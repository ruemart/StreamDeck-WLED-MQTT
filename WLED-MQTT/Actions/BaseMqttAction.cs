using BarRaider.SdTools;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using WLED_MQTT.Mqtt;

namespace WLED_MQTT.Actions
{
    public abstract class BaseMqttAction<TSettings> : PluginBase where TSettings : BasePluginSettings, new()
    {
        protected TSettings settings;

        protected readonly MqttClient mqttClient;

        public BaseMqttAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            mqttClient = new MqttClient();
            mqttClient.StatusChanged += OnMqttClientStatusChanged;
            mqttClient.StartMqttClientAsync(settings);
        }

        protected virtual void OnMqttClientStatusChanged(object sender, MqttClientStatusChangedEventHandler e) { }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Destructor called");
            mqttClient.StatusChanged -= OnMqttClientStatusChanged;
            mqttClient.Dispose();
        }

        public override async void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            await mqttClient.StopMqttClientAsync();
            await mqttClient.StartMqttClientAsync(settings);
            await SaveSettings();
        }

        protected Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }
    }
}
