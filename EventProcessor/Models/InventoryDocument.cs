using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventProcessor.Models
{
    public class InventoryDocument
    {
        public string id { get; set; }
        public string productName { get; set; }
        public JObject stock { get; set; }

        public static implicit operator DocumentResponse<object>(InventoryDocument v)
        {
            throw new NotImplementedException();
        }
    }
}
