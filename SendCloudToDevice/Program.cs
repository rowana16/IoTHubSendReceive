using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using System.Configuration;

namespace SendCloudToDevice
{
    class Program
    {
        static ServiceClient serviceClient;
        static string connectionString = ConfigurationManager.AppSettings["connectionString"];
        static string DeviceId = ConfigurationManager.AppSettings["DeviceId"];
        
        private async static Task SendCloudToDeviceMessageAsync()
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes("Cloud to device message."));
            await serviceClient.SendAsync(DeviceId , commandMessage);
        }

        private async static void ReceiveFeedbackAsync()
        {
            var feedbackReceiver = serviceClient.GetFeedbackReceiver();

            Console.WriteLine("\nReceiving c2d feedback from service");
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received feedback: {0}", string.Join(", ", feedbackBatch.Records.Select(f => f.StatusCode)));
                Console.ResetColor();

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Send Cloud-to-Device message\n");
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            ReceiveFeedbackAsync();
            Console.WriteLine("Press any key to send a C2D message.");
            Console.ReadLine();
            SendCloudToDeviceMessageAsync().Wait();
            Console.ReadLine();
        }
    }
}
