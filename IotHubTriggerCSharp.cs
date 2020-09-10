using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading.Tasks;

namespace Company.Function
{
    public static class IotHubTriggerCSharp
    {
        private static HttpClient client = new HttpClient();

        [FunctionName("IotHubTrigger")]
        public static async Task Run(
        [IoTHubTrigger("messages/events", Connection = "IoTHubEndpoint")]EventData message,
        [SignalR(HubName = "MyHub")]IAsyncCollector<SignalRMessage> signalRMessages,      
        ILogger log)
        {
            var messageFromIoTDevice = Encoding.UTF8.GetString(message.Body.Array);
            
            var deviceId = message.SystemProperties["iothub-connection-device-id"];

            log.LogInformation(deviceId.ToString());
            log.LogInformation($"C# IoT Hub trigger function processed a message: {messageFromIoTDevice}");
        
            string mensaje = string.Format("{0}|{1}",deviceId.ToString(), messageFromIoTDevice);
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "iotMessage",
                    Arguments = new[] { mensaje }
                })
                .ConfigureAwait(false);
        }

         [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "MyHub")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }
    }
}