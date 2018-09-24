using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MHArmory.ArmoryDataSource.DataStructures
{
    public class WeaponSlotRank
    {
        [JsonProperty("rank")]
        public int Rank { get; set; }
    }

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
        [JsonProperty("coatings")]
        public IList<string> Coatings { get; set; }
        [JsonProperty("phialType")]
        public string PhialType { get; set; }
        [JsonProperty("shellingType")]
        public string ShellingType { get; set; }
        [JsonProperty("boostType")]
        public string BoostType { get; set; }
        [JsonProperty("deviation")]
        public string Deviation { get; set; }
        [JsonProperty("specialAmmo")]
        public string SpecialAmmo { get; set; }

        // ammoCapacities
    }

    public class WeaponElementPrimitive
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("damage")]
        public int Value { get; set; }
        [JsonProperty("hidden")]
        public bool IsHidden { get; set; }
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
        [JsonProperty("slots")]
        public IList<WeaponSlotRank> Slots { get; set; }
        [JsonProperty("elements")]
        public IList<WeaponElementPrimitive> Elements { get; set; }

        // ...

        [JsonProperty("crafting")]
        public WeaponCraftingPrimitive Crafting { get; set; }
    }
}
