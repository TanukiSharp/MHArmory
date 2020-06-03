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
        [JsonProperty("crafting")]
        public CraftingPrimitive Crafting { get; set; }
    }

    internal class CharmPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
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
        public Dictionary<string, string> Name { get; private set; } = new Dictionary<string, string>();
        public int Level { get; }
        public int Rarity { get; }
        public int[] Slots { get; } = Array.Empty<int>();
        public IAbility[] Abilities { get; }
        public IEvent Event { get; }


        public ICraftMaterial[] CraftMaterials { get; }

        internal CharmLevel(int id, CharmLevelPrimitive currentCharmLevelPrimitive, IAbility[] abilities, ICraftMaterial[] materials)
        {
            Id = id;
            Level = currentCharmLevelPrimitive.Level;
            Rarity = currentCharmLevelPrimitive.Rarity;
            Abilities = abilities;
            CraftMaterials = materials;
            AddLocalization("EN", currentCharmLevelPrimitive);
        }

        internal void AddLocalization(string languageKey, CharmLevelPrimitive primitive)
        {
            if (primitive.Name != null)
                Name[languageKey] = primitive.Name;
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
    
    internal class Charm : ICharm
    {
        public int Id { get; private set; }

        public Dictionary<string, string> Name { get; private set; } = new Dictionary<string, string>();

        public ICharmLevel[] Levels { get; private set; }

        internal Charm(CharmPrimitive primitive)
        {
            Id = primitive.Id;
            Levels = null;
            Name["EN"] = primitive.Name;
        }

        internal void SetCharmLevels(ICharmLevel[] levels)
        {
            Levels = levels;
        }

        internal void AddLocalization(string languageKey, CharmPrimitive primitive)
        {
            if (primitive.Name != null)
                Name[languageKey] = primitive.Name;
            if(primitive.Levels.Count != Levels.Length)
                throw new InvalidOperationException($"Charm with ID '{primitive.Id}' in language '{languageKey}' has different number of levels ('{Levels.Length}' vs '{primitive.Levels.Count}')");

            foreach (CharmLevelPrimitive charmLevelPrimitive in primitive.Levels)
            {
                bool found = false;
                foreach(CharmLevel level in Levels)
                {
                    if(level.Level == charmLevelPrimitive.Level)
                    {
                        found = true;
                        level.AddLocalization(languageKey, charmLevelPrimitive);
                        break;
                    }
                }
                if (!found)
                    throw new InvalidOperationException($"Charm with ID '{primitive.Id}' in language '{languageKey}' has charm level with level '{charmLevelPrimitive.Level}' which is not present in the original");
            }
        }
    }
}
