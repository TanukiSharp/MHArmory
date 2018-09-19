using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MHArmory.ArmoryDataSource.DataStructures
{
    public class WeaponAttackPrimitve
    {
        [JsonProperty("display")]
        public int Display { get; set; }
    }

    public class WeaponAttributesPrimitive
    {
        [JsonProperty("damageType")]
        public string DamageType { get; set; }
        [JsonProperty("elderseal")]
        public string Elderseal { get; set; }
        [JsonProperty("affinity")]
        public int Affinity { get; set; }
        [JsonProperty("defense")]
        public int Defense { get; set; }
    }

    public class WeaponDurabilityPrimitive
    {
        [JsonProperty("red")]
        public int Red { get; set; }
        [JsonProperty("orange")]
        public int Orange { get; set; }
        [JsonProperty("yellow")]
        public int Yellow { get; set; }
        [JsonProperty("green")]
        public int Green { get; set; }
        [JsonProperty("blue")]
        public int Blue { get; set; }
        [JsonProperty("white")]
        public int White { get; set; }
    }

    public class WeaponCraftingPrimitive
    {
        [JsonProperty("craftable")]
        public bool IsCraftable { get; set; }
        [JsonProperty("previous")]
        public int? Previous { get; set; }
        [JsonProperty("branches")]
        public IList<int> Branches { get; set; }
    }

    public class WeaponPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("rarity")]
        public int Rarity { get; set; }
        [JsonProperty("attack")]
        public WeaponAttackPrimitve Attack { get; set; }
        [JsonProperty("attributes")]
        public WeaponAttributesPrimitive Attributes { get; set; }
        [JsonProperty("durability")]
        public WeaponDurabilityPrimitive[] SharpnessLevels { get; set; }

        // slots
        // ...

        [JsonProperty("crafting")]
        public WeaponCraftingPrimitive Crafting { get; set; }
    }
}
