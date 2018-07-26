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

        public Skill(int id, SkillPrimitive skillPrimitive)
        {
            Id = id;
            Name = skillPrimitive.Name;
            Description = "(no description)";
            Abilities = Enumerable.Range(1, skillPrimitive.MaxLevel)
                .Select(i => new Ability(uniqueAbilityId++, this, i, $"{Name} level {i}"))
                .ToArray();
        }

        public Skill(int id, string name, IAbility ability)
        {
            Id = id;
            Name = name;
            Description = "(no description)";
            Abilities = new IAbility[] { ability };
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public IAbility[] Abilities { get; }

        public override string ToString()
        {
            return $"{Name} ({Abilities.Length} abilities)";
        }
    }
}
