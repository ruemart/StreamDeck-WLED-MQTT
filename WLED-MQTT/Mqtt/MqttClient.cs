using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MQTTnet.Client.Publishing;

namespace WLED_MQTT.Mqtt
{
    public sealed class MqttClient : IDisposable
    {
        private readonly IManagedMqttClient mqttClient;
        private bool disposedValue;

        public MqttStatus Status { get; private set; }

        public event EventHandler<MqttClientStatusChangedEventHandler> StatusChanged;

        public MqttClient()
        {
            mqttClient = new MqttFactory().CreateManagedMqttClient();
            mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(MqttClientConnected);
            mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(MqttClientConnectionFailed);
            mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(MqttClientDisconnected);

            Status = MqttStatus.NotRunning;
            StatusChanged?.Invoke(this, new MqttClientStatusChangedEventHandler { NewStatus = Status });
        }

        /// <summary>
        /// Callback if the connection to the broker is lost.
        /// </summary>
        /// <param name="args"></param>
        private void MqttClientDisconnected(MqttClientDisconnectedEventArgs args)
        {
            if(mqttClient.IsStarted)
            {
                mqttClient.StopAsync();
            }

            Status = MqttStatus.NotRunning;
            StatusChanged?.Invoke(this, new MqttClientStatusChangedEventHandler { NewStatus = Status });
        }

        /// <summary>
        /// Callback if connecting to the broker failed.
        /// </summary>
        /// <param name="args"></param>
        private void MqttClientConnectionFailed(ManagedProcessFailedEventArgs args)
        {
            if (mqttClient.IsStarted)
            {
                mqttClient.StopAsync();
            }

            Status = MqttStatus.Faulty;
            StatusChanged?.Invoke(this, new MqttClientStatusChangedEventHandler { NewStatus = Status });
        }

        /// <summary>
        /// Callback if connecting to the broker was successful.
        /// </summary>
        /// <param name="args"></param>
        private void MqttClientConnected(MqttClientConnectedEventArgs args)
        {
            Status = MqttStatus.Running;
            StatusChanged?.Invoke(this, new MqttClientStatusChangedEventHandler { NewStatus = Status });
        }

        /// <summary>
        /// Starts the MQTT client if it is not running.
        /// </summary>
        /// <param name="settings">Setting used to configure the MQTT client.</param>
        public Task StartMqttClientAsync(BasePluginSettings settings)
        {
            if(mqttClient.IsStarted || mqttClient.IsConnected)
            {
                return Task.CompletedTask;
            }

            Status = MqttStatus.Connecting;
            StatusChanged?.Invoke(this, new MqttClientStatusChangedEventHandler { NewStatus = Status });

            return mqttClient.StartAsync(BuildMqttClientOptions(settings));
        }

        /// <summary>
        /// Stops the MQTT client if it is running.
        /// </summary>
        public Task StopMqttClientAsync()
        {
            if (!mqttClient.IsStarted || !mqttClient.IsConnected)
            {
                return Task.CompletedTask;
            }

            return mqttClient.StopAsync();
        }

        /// <summary>
        /// Sends <paramref name="payload"/> to <paramref name="topic"/>.
        /// </summary>
        /// <param name="topic">The topic to send the <paramref name="payload"/> to.</param>
        /// <param name="payload">The payload to send.</param>
        /// <param name="payload">Whether the message should be sent in retain mode.</param>
        /// <param name="serializerSettings">Serializer settings used to convert <paramref name="payload"/> into JSON format.</param>
        /// <returns><see langword="true"/> if sending <paramref name="payload"/> to <paramref name="topic"/> succeeded, <see langword="false"/> otherwise</returns>
        public Task<bool> SendAsync(string topic, object payload, bool retain = false, JsonSerializerSettings serializerSettings = null)
        {
            var strValue = JsonConvert.SerializeObject(payload, Formatting.None, serializerSettings);
            var mqttMessage = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(strValue).WithRetainFlag(retain).Build();
            return mqttClient.PublishAsync(mqttMessage).ContinueWith(task => task.Result.ReasonCode == MqttClientPublishReasonCode.Success);
        }

        private ManagedMqttClientOptions BuildMqttClientOptions(BasePluginSettings settings)
        {
            var clientOptions = new MqttClientOptionsBuilder();
            switch (settings.ConnectionType)
            {
                case ConnectionType.TCP:
                    clientOptions.WithTcpServer(settings.Host, settings.Port);
                    break;
                case ConnectionType.SecureTCP:
                    clientOptions.WithTcpServer(settings.Host, settings.Port).WithTls();
                    break;
                case ConnectionType.SecureTCPWithCertificate:
                    clientOptions.WithTcpServer(settings.Host, settings.Port)
                        .WithTls(new MqttClientOptionsBuilderTlsParameters
                        {
                            UseTls = true,
                            Certificates = new List<X509Certificate>
                            {
                                new X509Certificate(settings.CertificateFile),
                            },
                        });
                    break;
                case ConnectionType.WebSockets:
                    clientOptions.WithWebSocketServer(settings.WebSocketServerAdress);
                    break;
            }
            if (settings.CommunicationTimeout > 0)
            {
                clientOptions.WithCommunicationTimeout(new TimeSpan(0, 0, settings.CommunicationTimeout));
            }
            if (!string.IsNullOrWhiteSpace(settings.ClientId))
            {
                clientOptions.WithClientId(settings.ClientId);
            }
            if (!string.IsNullOrWhiteSpace(settings.User) && !string.IsNullOrWhiteSpace(settings.Password))
            {
                clientOptions.WithCredentials(settings.User, settings.Password);
            }
            return new ManagedMqttClientOptionsBuilder().WithClientOptions(clientOptions.Build()).Build();
        }

        private async void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                await StopMqttClientAsync();
                mqttClient?.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
