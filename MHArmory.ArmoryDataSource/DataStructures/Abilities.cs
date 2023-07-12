using System;
using System.Collections.Generic;
using System.Text;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using Newtonsoft.Json;

namespace MHArmory.ArmoryDataSource.DataStructures
{
    public class AbilityPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("name")]
        public Dictionary<string, string> Name { get; set; }
        [JsonProperty("description")]
        public Dictionary<string, string> Description { get; set; }
    }

    public class Ability : IAbility
    {
        public int Id { get; }
        public int Level { get; }
        public Dictionary<string, string> Name { get; }
        public Dictionary<string, string> Description { get; }
        public ISkill Skill { get; private set; }

        public Ability(AbilityPrimitive primitive)
        {
            Id = primitive.Id;
            Level = primitive.Level;
            Name = primitive.Name;
            Description = primitive.Description;
        }

        internal void Update(ISkill skill)
        {
            Skill = skill;
        }

        public override string ToString()
        {
            return $"{Localization.GetDefault(Skill.Name)} level {Level} ({Localization.GetDefault(Name)}, {Localization.GetDefault(Description)})";
        }
    }
}
