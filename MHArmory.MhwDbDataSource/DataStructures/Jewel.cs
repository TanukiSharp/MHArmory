using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;
using Newtonsoft.Json;

namespace MHArmory.MhwDbDataSource.DataStructures
{
    internal class JewelPrimitive
    {
        [JsonProperty("id")]
        internal int Id { get; set; }
        [JsonProperty("name")]
        internal string Name { get; set; }
        [JsonProperty("rarity")]
        internal int Rarity { get; set; }
        [JsonProperty("slot")]
        internal int SlotSize { get; set; }
        [JsonProperty("skills")]
        internal IList<AbilityIdPrimitive> Abilitites { get; set; }
    }

    internal class Jewel : IJewel
    {
        public int Id { get; private set; }
        public Dictionary<string, string> Name { get; private set; } =  new Dictionary<string, string>();
        public int Rarity { get; private set; }
        public int SlotSize { get; private set; }
        public IAbility[] Abilities { get; private set; }


        internal Jewel(JewelPrimitive primitive, IAbility[] abilities)
        {
            Id = primitive.Id;
            Rarity = primitive.Rarity;
            SlotSize = primitive.SlotSize;
            Abilities = abilities;
            AddLocalization("EN", primitive);
        }

        internal void AddLocalization(string languageKey, JewelPrimitive primitive)
        {
            if(primitive.Name != null)
                Name[languageKey] = primitive.Name;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
