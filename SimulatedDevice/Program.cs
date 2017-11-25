namespace SimulatedDevice
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;
    using System.Configuration;

    public class Program
    {
        private readonly string IotHubUri = ConfigurationManager.AppSettings["IotHubUri"];
        private readonly string DeviceKey = ConfigurationManager.AppSettings["DeviceKey"];
        private readonly string DeviceId = ConfigurationManager.AppSettings["DeviceId"];
        private const double MinTemperature = 20;
        private const double MinHumidity = 60;
        private static readonly Random Rand = new Random();
        private static DeviceClient _deviceClient;
        private static int _messageId = 1;

        private static async void SendDeviceToCloudMessagesAsync()
        {
            string DeviceId = ConfigurationManager.AppSettings["DeviceId"];
            while (true)
            {
                var currentTemperature = MinTemperature + Rand.NextDouble() * 15;
                var currentHumidity = MinHumidity + Rand.NextDouble() * 20;

                var telemetryDataPoint = new
                {
                    messageId = _messageId++,
                    deviceId = DeviceId,
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                await _deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(1000);
            }
        }

        private static async void ReceiveC2dAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Message receivedMessage = await _deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received message: {0}", Encoding.ASCII.GetString(receivedMessage.GetBytes()));
                Console.ResetColor();

                await _deviceClient.CompleteAsync(receivedMessage);
            }
        }

        private static void Main(string[] args)
        {
            string DeviceId = ConfigurationManager.AppSettings["DeviceId"];
            string DeviceKey = ConfigurationManager.AppSettings["DeviceKey"];
            string IotHubUri = ConfigurationManager.AppSettings["IotHubUri"];

            Console.WriteLine("Simulated device\n");
            _deviceClient = DeviceClient.Create(IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey), TransportType.Mqtt);

            SendDeviceToCloudMessagesAsync();
            ReceiveC2dAsync();
            Console.ReadLine();
        }
    }
}
