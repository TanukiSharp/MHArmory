using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;
using Newtonsoft.Json;

namespace MHArmory.MhwDbDataSource.DataStructures
{
    internal class AbilityIdPrimitive
    {
        [JsonProperty("id")]
        public int AbilityId { get; set; }
    }

    internal class CharmLevelPrimitive
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("rarity")]
        public int Rarity { get; set; }
        [JsonProperty("skills")]
        public AbilityIdPrimitive[] Abilitites { get; set; }
    }

    internal class CharmPrimitive
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("ranks")]
        public CharmLevelPrimitive[] Levels { get; set; }
    }

    internal class CharmLevel : ICharmLevel
    {
        public string Name { get; }
        public int Level { get; }
        public int Rarity { get; }
        public IAbility[] Abilities { get; }

        internal CharmLevel(CharmLevelPrimitive currentCharmLevelPrimitive, IAbility[] abilities)
        {
            Name = currentCharmLevelPrimitive.Name;
            Level = currentCharmLevelPrimitive.Level;
            Rarity = currentCharmLevelPrimitive.Rarity;
            Abilities = currentCharmLevelPrimitive.Abilitites
                .Select(x => abilities.FirstOrDefault(a => a.Id == x.AbilityId))
                .ToArray();
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }

    internal class Charm : ICharm
    {
        public string Name { get; }
        public ICharmLevel[] Levels { get; }

        internal Charm(string name, ICharmLevel[] charmLevels)
        {
            Name = name;
            Levels = charmLevels;
        }

        public override string ToString()
        {
            return $"{Name} ({Levels.Length})";
        }
    }
}
