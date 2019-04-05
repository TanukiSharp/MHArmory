using System;
using System.Collections.Generic;
using System.Text;
using MHArmory.Core.DataStructures;
using Newtonsoft.Json;

namespace MHArmory.ArmoryDataSource.DataStructures
{
    public class ArmorAttributesPrimitive
    {
        [JsonProperty("gender")]
        public Gender Gender { get; set; }
    }

    public class ArmorDefensePrimitive
    {
        [JsonProperty("base")]
        public int Base { get; set; }
        [JsonProperty("max")]
        public int Max { get; set; }
        [JsonProperty("augmented")]
        public int Augmented { get; set; }
    }

    public class ArmorResistancesPrimitive
    {
        [JsonProperty("fire")]
        public int Fire { get; set; }
        [JsonProperty("water")]
        public int Water { get; set; }
        [JsonProperty("thunder")]
        public int Thunder { get; set; }
        [JsonProperty("ice")]
        public int Ice { get; set; }
        [JsonProperty("dragon")]
        public int Dragon { get; set; }
    }

    public class ArmorPiecePrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public Dictionary<string, string> Name { get; set; }
        [JsonProperty("rarity")]
        public int Rarity { get; set; }
        [JsonProperty("slots")]
        public IList<int> Slots { get; set; }
        [JsonProperty("abilityIds")]
        public IList<int> AbilityIds { get; set; }
        [JsonProperty("defense")]
        public ArmorDefensePrimitive Defense { get; set; }
        [JsonProperty("resistances")]
        public ArmorResistancesPrimitive Resistances { get; set; }
        [JsonProperty("attributes")]
        public ArmorAttributesPrimitive Attributes { get; set; }
        [JsonProperty("armorSetSkillIds")]
        public List<int> ArmorSetSkillIds { get; set; }
        [JsonProperty("fullArmorSetId")]
        public int? FullArmorSetId { get; set; }
        [JsonProperty("eventId")]
        public int? EventId { get; set; }
        [JsonProperty("craftMaterials")]
        public CraftMaterial[] CraftMaterials { get; set; }
    }
}
