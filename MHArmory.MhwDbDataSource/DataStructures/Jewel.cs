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
        internal AbilityIdPrimitive[] Abilitites { get; set; }
    }

    internal class Jewel : IJewel
    {
        public int Id { get; }
        public string Name { get; }
        public int Rarity { get; }
        public int SlotSize { get; }
        public IAbility[] Abilities { get; }

        internal Jewel(JewelPrimitive primitive, IAbility[] abilities)
        {
            Id = primitive.Id;
            Name = primitive.Name;
            Rarity = primitive.Rarity;
            SlotSize = primitive.SlotSize;
            Abilities = primitive.Abilitites.Select(x => abilities.FirstOrDefault(y => x.AbilityId == y.Id)).ToArray();
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
