using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MHArmory.ArmoryDataSource.DataStructures
{
    public class CharmLevelPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public Dictionary<string, string> Name { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("rarity")]
        public int Rarity { get; set; }
        [JsonProperty("abilityIds")]
        public IList<int> AbilityIds { get; set; }
        [JsonProperty("slots")]
        public IList<int> Slots { get; set; }
        [JsonProperty("eventId")]
        public int? EventId { get; set; }
        [JsonProperty("craftMaterials")]
        public CraftMaterial[] CraftMaterials { get; set; }
    }
}
