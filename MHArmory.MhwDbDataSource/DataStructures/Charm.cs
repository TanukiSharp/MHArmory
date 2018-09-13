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
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("skill")]
        public int SkillId { get; set; }
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
        public IList<AbilityIdPrimitive> Abilitites { get; set; }
    }

    internal class CharmPrimitive
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("ranks")]
        public IList<CharmLevelPrimitive> Levels { get; set; }
    }

    internal class CharmLevel : ICharmLevel
    {
        public ICharm Charm { get; private set; }
        public int Id { get; }
        public EquipmentType Type { get; } = EquipmentType.Charm;
        public string Name { get; }
        public int Level { get; }
        public int Rarity { get; }
        public int[] Slots { get; } = Array.Empty<int>();
        public IAbility[] Abilities { get; }

        internal CharmLevel(int id, CharmLevelPrimitive currentCharmLevelPrimitive, IAbility[] abilities)
        {
            Id = id;
            Name = currentCharmLevelPrimitive.Name;
            Level = currentCharmLevelPrimitive.Level;
            Rarity = currentCharmLevelPrimitive.Rarity;
            Abilities = abilities;
        }

        public void UpdateCharm(ICharm charm)
        {
            Charm = charm;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
