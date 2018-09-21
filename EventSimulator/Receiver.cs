using EventProcessor.Models;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EventSimulator
{
    public static class Receiver
    {
        private static EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(Environment.GetEnvironmentVariable("EventHubsConnectionString"));

        [FunctionName("Receiver")]
        public static async Task RunAsync([ServiceBusTrigger("events", Connection = "ServiceBusConnectionString")]string inputEvent, ILogger log)
        {
            log.LogInformation($"Triggered to send event {inputEvent}");

            await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(inputEvent)));
        }
    }
}
