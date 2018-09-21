using EventProcessor.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventProcessor
{
    public static class EventProcessor
    {
        // Azure Function to process inventory events
        [FunctionName(nameof(UpdateInventory))]

        public static async Task UpdateInventory(
            [EventHubTrigger("inventory-transactions", Connection = "EventHubsConnectionString", ConsumerGroup = "cloud")]InventoryEvent inventoryEvent,
            [SignalR(HubName = "inventory")]IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            log.LogInformation($"Processed a message: {inventoryEvent.type} at {inventoryEvent.time}");

            // Get the inventory information from CosmosDB
            var doc = await client.ReadDocumentAsync<InventoryDocument>(documentUrlBase + inventoryEvent.productId);

            // Modify the inventory based on the event
            doc = string.Equals(inventoryEvent.type, "sell") ? removeInventory(inventoryEvent, doc) : addInventory(inventoryEvent, doc);

            // Update the inventory in CosmosDB
            var res = await client.ReplaceDocumentAsync(documentUrlBase + inventoryEvent.productId, doc.Document);

            // Notify the dashboard
            await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = "inventory",
                        Arguments = new[] { GenerateInventoryMessage(doc.Document) }
                    });
        }




        // Decrement the inventory for a sell
        private static DocumentResponse<InventoryDocument> removeInventory(InventoryEvent e, DocumentResponse<InventoryDocument> doc)
        {
            if (doc.Document.stock == null)
            {
                doc.Document.stock = new JObject();
            }
            doc.Document.stock[e.location] = ((int?)doc.Document.stock[e.location] ?? 0) - e.count;
            return doc;
        }

        // Increment the inventory for a purchase
        private static DocumentResponse<InventoryDocument> addInventory(InventoryEvent e, DocumentResponse<InventoryDocument> doc)
        {
            if (doc.Document.stock == null)
            {
                doc.Document.stock = new JObject();
            }
            doc.Document.stock[e.location] = ((int?)doc.Document.stock[e.location] ?? 0) + e.count;
            return doc;
        }

        // Generate SignalR Message
        private static string GenerateInventoryMessage(InventoryDocument document)
        {
            var message = new JObject();
            message["Product"] = document.id;
            foreach (var location in document.stock)
            {
                message[location.Key] = location.Value;
            }
            return message.ToString();
        }

        private static string EndpointUrl = Environment.GetEnvironmentVariable("CosmosDbEndpointUrl");
        private static string PrimaryKey = Environment.GetEnvironmentVariable("CosmosDbPrimaryKey");
        private static string db = "inventoryDb";
        private static string collection = "inventory";
        private static string documentUrlBase = $"dbs/{db}/colls/{collection}/docs/";

        private static DocumentClient client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);

        /// <summary>
        /// Function to negotiate the connection with SignalR service hub.
        /// </summary>
        [FunctionName("negotiate")]
        public static IActionResult GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequest req,
            [SignalRConnectionInfo(HubName = "inventory")]SignalRConnectionInfo connectionInfo)
        {
            return new OkObjectResult(connectionInfo);
        }
    }
}
