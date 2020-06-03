using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;
using Newtonsoft.Json;

namespace MHArmory.MhwDbDataSource.DataStructures
{
    internal class AbilityPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }

    internal class SkillPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("ranks")]
        public IList<AbilityPrimitive> Abilities { get; set; }
    }

    internal class Ability : IAbility
    {
        public int Id { get; private set; }

        public ISkill Skill { get; private set; }

        public int Level { get; private set; }

        public Dictionary<string, string> Description { get; private set; } = new Dictionary<string, string>();

        internal Ability(AbilityPrimitive primitive, ISkill skill)
        {
            Id = primitive.Id;
            Level = primitive.Level;
            Skill = skill;
        }

        internal void AddLocalization(string languageKey, AbilityPrimitive primitive)
        {
            if (primitive.Id != Id)
            {
                throw new InvalidOperationException($"Tried to add localization of ability primitive with ID '{primitive.Id}' to ability with ID '{Id}'");
            }
            if(primitive.Description != null)
                Description[languageKey] = primitive.Description;
        }
    }

    internal class Skill : ISkill
    {
        public int Id { get; private set; }
        public Dictionary<string, string> Name { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Description { get; private set; } = new Dictionary<string, string>();
        public int MaxLevel { get; private set; }
        public string[] Categories { get; private set; }
        public IAbility[] Abilities { get; private set; }

        internal Skill(SkillPrimitive primitive)
        {
            Id = primitive.Id;
            Abilities = new IAbility[primitive.Abilities.Count];
            for (int i = 0; i < primitive.Abilities.Count; ++i)
                Abilities[i] = new Ability(primitive.Abilities[i], this);
            MaxLevel = Abilities.Max(x => x.Level);
            AddLocalization("EN", primitive);
        }

        internal void AddLocalization(string languageKey, SkillPrimitive primitive)
        {
            if (primitive.Id != Id)
            {
                throw new InvalidOperationException($"Tried to add localization of skill primitive with ID '{primitive.Id}' to skill with ID '{Id}'");
            }
            if(primitive.Name != null)
                Name[languageKey] = primitive.Name;
            if(primitive.Description != null)
                Description[languageKey] = primitive.Description;
            if (Abilities.Length != primitive.Abilities.Count)
            {
                throw new InvalidOperationException($"Skill with ID '{primitive.Id}' in language '{languageKey}' has different number of abilities ('{Abilities.Length}' vs '{primitive.Abilities.Count}')");
            }
            foreach (AbilityPrimitive abilityPrimitive in primitive.Abilities)
            {
                bool found = false;
                foreach (Ability ability in Abilities)
                {
                    if (ability.Id == abilityPrimitive.Id)
                    {
                        found = true;
                        ability.AddLocalization(languageKey, abilityPrimitive);
                        break;
                    }
                }
                if (!found)
                {
                    throw new InvalidOperationException($"Skill with ID '{primitive.Id}' in language '{languageKey}' has ability with ID '{abilityPrimitive.Id}' which is not present in the original");
                }
            }
        }

        public override string ToString()
        {
            return $"{Name} [{Abilities.Length} levels] ({Description})";
        }
    }
}
