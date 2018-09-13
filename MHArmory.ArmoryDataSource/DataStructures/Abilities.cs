using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MHArmory.DataSource.DataStructures
{
    public class AbilityPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
