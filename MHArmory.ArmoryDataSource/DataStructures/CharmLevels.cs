using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MHArmory.DataSource.DataStructures
{
    public class CharmLevelPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("rarity")]
        public int Rarity { get; set; }
        [JsonProperty("abilityIds")]
        public IList<int> AbilityIds { get; set; }
    }
}
