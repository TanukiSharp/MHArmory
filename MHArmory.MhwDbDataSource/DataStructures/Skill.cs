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
        [JsonProperty("skillId")]
        public int SkillId { get; set; }
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
        public int MaxLevel { get; private set; }
        public IAbility[] Abilities { get; private set; }

        internal Skill(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        internal void SetAbilities(IAbility[] abilities)
        {
            MaxLevel = abilities.Max(x => x.Level);
            Abilities = abilities;
        }

        public override string ToString()
        {
            return $"{Name} [{Abilities.Length} levels] ({Description})";
        }
    }
}
