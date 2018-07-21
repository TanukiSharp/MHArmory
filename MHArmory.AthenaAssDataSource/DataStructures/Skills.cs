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
                .Select(i =>
                {
                    int aid = uniqueAbilityId++;
                    return new Ability(aid, this, i, $"{Name} level {i}");
                })
                .ToArray();
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public IAbility[] Abilities { get; }
    }
}
