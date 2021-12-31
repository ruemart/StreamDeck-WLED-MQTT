using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

        private MqttStatus currentMqttStatus;

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

        protected override void OnMqttClientStatusChanged(object sender, MqttClientStatusChangedEventHandler e)
        {
            currentMqttStatus = e.NewStatus;
            switch (currentMqttStatus)
            {
                case MqttStatus.NotRunning:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ChangeFX: MQTT Offline");
                    break;
                case MqttStatus.Connecting:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ChangeFX: MQTT Connecting");
                    break;
                case MqttStatus.Faulty:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ChangeFX: MQTT Failed");
                    break;
                case MqttStatus.Running:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ChangeFX: MQTT Running");
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
                        background = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "Images", "EffectOffline@2x.png"));
                        break;
                    case MqttStatus.Connecting:
                        background = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "Images", "EffectConnecting@2x.png"));
                        break;
                    case MqttStatus.Faulty:
                        background = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "Images", "EffectError@2x.png"));
                        break;
                    case MqttStatus.Running:
                        background = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "Images", "EffectOnline@2x.png"));
                        break;
                }
                graphics.DrawImage(background, 0, 0, bmp.Width, bmp.Height);

                var font = new Font(FontFamily.GenericSansSerif, 40, FontStyle.Bold, GraphicsUnit.Pixel);
                var fontColor = new SolidBrush(Color.White);
                var text = settings.Effects.First(e => e.EffectId == settings.SelectedEffect).EffectName;

                var textParts = text.Split(' ');
                var stringPosY = 32.0f;
                var fittingFontSize = float.MaxValue;
                foreach (var part in textParts)
                {
                    var currentPartFontSize = graphics.GetFontSizeWhereTextFitsImage(part, bmp.Width - 10, font);
                    if(currentPartFontSize < fittingFontSize)
                    {
                        fittingFontSize = currentPartFontSize;
                    }
                }

                using (var measureFont = new Font(font.FontFamily, fittingFontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    foreach (var part in textParts)
                    {
                        var textSize = graphics.MeasureString(part, measureFont);
                        var stringPosX = (bmp.Width - textSize.Width) / 2;
                        graphics.DrawString(part, measureFont, fontColor, new PointF(stringPosX, stringPosY));
                        stringPosY += textSize.Height;
                    }
                }

                await Connection.SetImageAsync(bmp);
                graphics.Dispose();
                font.Dispose();
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
            var oldFx = settings.SelectedEffect;
            Tools.AutoPopulateSettings(settings, payload.Settings);
            if (oldFx != settings.SelectedEffect)
            {
                ChangeKeyImage(currentMqttStatus);
            }
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
