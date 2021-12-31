using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
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

        public class PluginSettings : BaseSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings
                {
                    Colors = "#FF0000",
                    Mode = Mode.Manual,
                    RotationSpeed = 60,
                    ClientId = Guid.NewGuid().ToString(),
                };
                return instance;
            }

            /// <summary>
            /// Gets or sets the color to set.
            /// </summary>
            [JsonProperty(PropertyName = "colors")]
            public string Colors { get; set; } = "#FF0000";

            /// <summary>
            /// Gets or sets whether the colors change automatically or via key press.
            /// </summary>
            [JsonProperty(PropertyName = "mode")]
            public Mode Mode { get; set; } = Mode.Manual;

            /// <summary>
            /// Gets or sets the time between automatic color changes.
            /// </summary>
            [JsonProperty(PropertyName = "rotationSpeed")]
            public int RotationSpeed { get; set; } = 60;

            public List<string> ColorList { get; set; } = new List<string>();

            public int ColorIndex { get; set; } = 0;
        }

        private readonly Timer timer;
        private MqttStatus currentMqttStatus;

        public ChangeColorAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                settings = PluginSettings.CreateDefaultSettings();
            }
            else
            {
                settings = payload.Settings.ToObject<PluginSettings>();
            }

            timer = new Timer();
            timer.Elapsed += OnTimerElapsed;
            timer.Interval = settings.RotationSpeed;
            settings.ColorList = new List<string>(settings.Colors.Split(','));
            SaveSettings();
            Connection.GetGlobalSettingsAsync();
        }

        protected override void OnMqttClientStatusChanged(object sender, MqttClientStatusChangedEventHandler e)
        {
            currentMqttStatus = e.NewStatus;
            switch (currentMqttStatus)
            {
                case MqttStatus.NotRunning:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ChangeColor: MQTT Offline");
                    break;
                case MqttStatus.Connecting:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ChangeColor: MQTT Connecting");
                    break;
                case MqttStatus.Faulty:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ChangeColor: MQTT Failed");
                    break;
                case MqttStatus.Running:
                    Logger.Instance.LogMessage(TracingLevel.INFO, $"ChangeColor: MQTT Running");
                    break;
            }
            ChangeKeyImage(currentMqttStatus);
        }

        public override void Dispose() { }

        public override void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");
        }

        public override void KeyReleased(KeyPayload payload)
        {
            if (settings.Mode == Mode.Manual)
            {
                SendNextColorToMqtt();
            }
            else if (settings.Mode == Mode.Automatic)
            {
                if (timer.Enabled)
                {
                    timer.Stop();
                }
                else
                {
                    SendNextColorToMqtt();
                    timer.Start();
                }
            }
        }

        public override void OnTick() { }

        public override async void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            var oldColors = settings.Colors;
            var oldRotationSpeed = settings.RotationSpeed;
            Tools.AutoPopulateSettings(settings, payload.Settings);

            if (oldColors != settings.Colors)
            {
                settings.ColorList = new List<string>(settings.Colors.Split(','));
                settings.ColorIndex = 0;
                ChangeKeyImage(currentMqttStatus);
            }

            if (oldRotationSpeed != settings.RotationSpeed)
            {
                var timerWasRunning = timer.Enabled;
                if (timerWasRunning)
                {
                    // stop it to update the interval.
                    timer.Stop();
                }
                timer.Interval = settings.RotationSpeed * 1000; // timer needs milliseconds.
                if (timerWasRunning)
                {
                    // turn it on again.
                    timer.Start();
                }
            }

            await SaveSettings();
            Tools.AutoPopulateSettings(globalSettings, payload.Settings);
            await SaveGlobalSettings();
        }

        protected override async void ChangeKeyImage(MqttStatus status)
        {
            using (var bmp = Tools.GenerateGenericKeyImage(out var graphics))
            {
                Image background = null;
                switch (status)
                {
                    case MqttStatus.NotRunning:
                        background = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "Images", "ColorOffline@2x.png"));
                        break;
                    case MqttStatus.Connecting:
                        background = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "Images", "ColorConnecting@2x.png"));
                        break;
                    case MqttStatus.Faulty:
                        background = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "Images", "ColorError@2x.png"));
                        break;
                    case MqttStatus.Running:
                        background = Image.FromFile(Path.Combine(Environment.CurrentDirectory, "Images", "ColorOnline@2x.png"));
                        break;
                }
                graphics.DrawImage(background, 0, 0, bmp.Width, bmp.Height);

                graphics.FillRectangle(new SolidBrush(ColorTranslator.FromHtml(settings.ColorList[settings.ColorIndex])), new Rectangle(bmp.Width / 2 - 38, bmp.Height / 2 - 25, 50, 50));
                graphics.DrawRectangle(new Pen(Color.White, 2), new Rectangle(bmp.Width / 2 - 38, bmp.Height / 2 - 25, 50, 50));

                var nextCol = settings.ColorIndex + 1 >= settings.ColorList.Count ? 0 : settings.ColorIndex + 1;
                graphics.FillRectangle(new SolidBrush(ColorTranslator.FromHtml(settings.ColorList[nextCol])), new Rectangle(bmp.Width / 2 + 12, bmp.Height / 2, 25, 25));
                graphics.DrawRectangle(new Pen(Color.White, 2), new Rectangle(bmp.Width / 2 + 12, bmp.Height / 2, 25, 25));

                await Connection.SetImageAsync(bmp);
                graphics.Dispose();
            }
        }

        protected override Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        private void SendNextColorToMqtt()
        {
            mqttClient.SendAsync(globalSettings.DeviceTopic + "/col", settings.ColorList[settings.ColorIndex]);
            settings.ColorIndex++;
            if (settings.ColorIndex >= settings.ColorList.Count)
            {
                settings.ColorIndex = 0;
            }
            ChangeKeyImage(currentMqttStatus);
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            SendNextColorToMqtt();
        }
    }
}