using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventProcessor.Models
{
    public class InventoryEvent
    {
        public string location { get; set; }
        public string type { get; set; }
        public int count { get; set; }
        public string productId { get; set; }
        public DateTime time { get; set; }

        public byte[] GetJsonBytes()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}
