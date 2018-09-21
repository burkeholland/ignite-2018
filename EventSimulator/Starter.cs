using EventProcessor.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSimulator
{
    public static class Starter
    {
        private static Random rnd = new Random();
        private static QueueClient sendClient = new QueueClient(Environment.GetEnvironmentVariable("ServiceBusConnectionString"), "events");

        [FunctionName("Starter")]
        public static async Task RunAsync(
            [ServiceBusTrigger("starter", Connection = "ServiceBusConnectionString")]string inputMessage,
            [CosmosDB("inventoryDb", "inventory", ConnectionStringSetting = "CosmosDbConnectionString", SqlQuery = "SELECT * FROM r")]IEnumerable<InventoryDocument> docs,
            ILogger log)
        {
            log.LogInformation($"Starter function triggered and requests # of messages: {inputMessage}");

            if (int.TryParse(inputMessage, out int messageCount))
            {
                var productNames = GetProductNames(docs);
                var locationNames = GetLocationNames(docs);
                var messages = new List<Message>();

                for (int x = 0; x < messageCount; x++)
                {
                    string type = rnd.Next(2) == 1 ? "sell" : "buy";
                    int maxCount = type.Equals("sell") ? 300 : 335;
                    InventoryEvent @event = new InventoryEvent
                    {
                        count = rnd.Next(1, maxCount),
                        location = locationNames[rnd.Next(0, locationNames.Count())],
                        productId = productNames[rnd.Next(0, productNames.Count())],
                        time = DateTime.UtcNow,
                        type = type
                    };

                    var message = new Message(@event.GetJsonBytes())
                    {
                        ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddSeconds(rnd.Next(0, 240))
                    };
                    messages.Add(message);
                }
                await sendClient.SendAsync(messages);
            }
            else
            {
                log.LogInformation($"Unable to parse int from {inputMessage}");
            }
        }

        private static IList<string> GetProductNames(IEnumerable<InventoryDocument> docs)
        {
            return (from d in docs
                    select d.id).ToList();
        }

        private static IList<string> GetLocationNames(IEnumerable<InventoryDocument> docs)
        {
            var locations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(var doc in docs)
            {
                foreach(var location in doc.stock)
                {
                    locations.Add(location.Key);
                }
            }

            return locations.ToList();
        }
    }
}
