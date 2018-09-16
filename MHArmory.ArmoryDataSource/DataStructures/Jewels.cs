using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MHArmory.ArmoryDataSource.DataStructures
{
    public class JewelPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("rarity")]
        public int Rarity { get; set; }
        [JsonProperty("slotSize")]
        public int SlotSize { get; set; }
        [JsonProperty("abilityIds")]
        public IList<int> AbilityIds { get; set; }
    }
}
