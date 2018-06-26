using System;
using System.Collections.Generic;
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
        [JsonProperty("skillId")]
        public int SkillId { get; set; }
    }

    internal class Ability : IAbility
    {
        public int Id { get; }
        public ISkill Skill { get; }
        public int Level { get; }
        public string Description { get; }

        internal Ability(int id, ISkill skill, int level, string description)
        {
            Id = id;
            Skill = skill;
            Level = level;
            Description = description;
        }

        public override int GetHashCode()
        {
            return Id;
        }
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

    internal class Skill : ISkill
    {
        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public IAbility[] Abilities { get; private set; }

        internal Skill(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        internal void SetAbilities(IAbility[] abilities)
        {
            Abilities = abilities;
        }
    }
}
