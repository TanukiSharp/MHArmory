using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MHArmory.ArmoryDataSource.DataStructures
{
    public class LocalizedItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("values")]
        public Dictionary<string, string> Values { get; set; }
    }

    public class CraftMaterial
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}
