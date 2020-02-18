using MHArmory.Core.DataStructures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.MhwDbDataSource.DataStructures
{
    internal class CraftingCostItemPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
    internal class CraftingCostPrimitive
    {
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
        [JsonProperty("item")]
        public CraftingCostItemPrimitive Item { get; set; }
    }

    internal class CraftingPrimitive
    {
        [JsonProperty("craftable")]
        public bool Craftable { get; set; }
        [JsonProperty("materials")]
        public CraftingCostPrimitive[] Materials { get; set; }
    }

    internal class ItemPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    internal class LocalizedItem : ILocalizedItem
    {
        public int Id { get; }

        public Dictionary<string, string> Values { get; } = new Dictionary<string, string>();

        internal LocalizedItem(ItemPrimitive primitive)
        {
            Id = primitive.Id;
            AddLocalization("EN", primitive);
        }

        internal void AddLocalization(string languageKey, ItemPrimitive primitive)
        {
            if (primitive.Name != null)
                Values[languageKey] = primitive.Name;
        }
    }
}
