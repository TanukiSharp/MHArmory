using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;

namespace MHArmory.AthenaAssDataSource.DataStructures
{
    internal class SkillPrimitive
    {
        internal string Name = null;
        [Name("Max Level")]
        internal int MaxLevel = 0;
    }

    internal class Skill : ISkill
    {
        private static int uniqueAbilityId = 1;

        public Skill(int id, string description, SkillPrimitive skillPrimitive)
        {
            Id = id;
            Name = skillPrimitive.Name;
            Description = description;
            MaxLevel = skillPrimitive.MaxLevel;
            Abilities = Enumerable.Range(1, skillPrimitive.MaxLevel)
                .Select(i => new Ability(uniqueAbilityId++, this, i, $"{Name} level {i}"))
                .ToArray();
        }

        public Skill(int id, string name, string description, IAbility ability)
        {
            Id = id;
            Name = name;
            Description = description;
            MaxLevel = ability.Skill.MaxLevel;
            Abilities = new IAbility[] { ability };
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; private set; }
        public int MaxLevel { get; }
        public IAbility[] Abilities { get; }

        public void UpdateDescription(string description)
        {
            Description = description;
        }

        public override string ToString()
        {
            return $"{Name} ({Abilities.Length} abilities)";
        }
    }
}
