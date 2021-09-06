using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;


namespace MqttDojo.WpfApp
{
    class MainWindowViewModel
    {
        private readonly IManagedMqttClient _mqttClient;

        public MainWindowViewModel()
        {
            // Creates a new client
            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder()
                                        .WithClientId("Dev.To")
                                        .WithCredentials("cheerytuna", "Z4Vh6_.fqDfJc-t")
                                        .WithTcpServer("3f7ae44382104c1fa902ecf1b633de4b.s2.eu.hivemq.cloud", 8883);
            // Create client options objects
            ManagedMqttClientOptions options = new ManagedMqttClientOptionsBuilder()
                                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(60))
                                    .WithClientOptions(builder.Build())
                                    .Build();


            // Creates the client object
            _mqttClient = new MqttFactory().CreateManagedMqttClient();
            // Set up handlers
            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
            _mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(OnConnectingFailed);
            
            _mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                try
                {
                    string topic = e.ApplicationMessage.Topic;

                    if (string.IsNullOrWhiteSpace(topic) == false)
                    {
                        string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                        Debug.WriteLine($"Topic: {topic}. Message Received: {payload}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message, ex);
                }
            });

            // Starts a connection with the Broker
            _mqttClient.StartAsync(options).GetAwaiter().GetResult();

            SubscribeAsync("dev.to/topic/json").GetAwaiter().GetResult();
            //task.Start();

            // Send a new message to the broker every second
            while (true)
            {
                string json = JsonConvert.SerializeObject(new { message = "Heyo :)", sent = DateTimeOffset.UtcNow });
                _mqttClient.PublishAsync("dev.to/topic/json", json);
                Task.Delay(1000).GetAwaiter().GetResult();

            }


        }

        public async Task SubscribeAsync(string topic, int qos = 1) =>
await _mqttClient.SubscribeAsync(new TopicFilterBuilder()
.WithTopic(topic)
.WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
.Build());

        private void OnConnectingFailed(ManagedProcessFailedEventArgs obj)
        {
        }

        private void OnDisconnected(MqttClientDisconnectedEventArgs obj)
        {
        }

        private void OnConnected(MqttClientConnectedEventArgs obj)
        {
        }
    }
}
