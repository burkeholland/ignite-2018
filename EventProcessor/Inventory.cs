
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Collections;
using EventProcessor.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace EventProcessor
{
    public static class Inventory
    {
        [FunctionName("Inventory")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "options",Route = null)]HttpRequest req, 
            [CosmosDB("inventoryDb", "inventory", ConnectionStringSetting = "CosmosDbConnectionString", SqlQuery = "SELECT * FROM r")]IEnumerable<InventoryDocument> docs,
            ILogger log)
        {
            log.LogInformation("Get inventory triggered.");

            var response = new JArray();
            foreach (var doc in docs)
            {
                var product = new JObject();
                product["Product"] = doc.id;
                foreach (var location in doc.stock)
                {
                    product[location.Key] = location.Value;
                }
                response.Add(product);
            }

            return new OkObjectResult(response);
        }

        [FunctionName("Keep_Alive")]
        public static void RunTrigger([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Poke... stay awake");
        }
    }
}
