using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MHArmory.DataSource.DataStructures
{
    public class CharmPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("levelIds")]
        public IList<int> LevelIds { get; set; }
    }
}
