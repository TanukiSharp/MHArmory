using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public interface IAbility
    {
        int Id { get; }
        ISkill Skill { get; }
        int Level { get; }
        string Description { get; }
    }

    public class Ability : IAbility
    {
        public Ability(int id, ISkill skill, int level, string description)
        {
            Id = id;
            Skill = skill;
            Level = level;
            Description = description;
        }

        public int Id { get; }
        public ISkill Skill { get; }
        public int Level { get; }
        public string Description { get; private set; }

        public void UpdateDescription(string description)
        {
            Description = description;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return $"{Skill.Name} level {Level} ({Description})";
        }
    }

    public interface ISkill
    {
        int Id { get; }
        string Name { get; }
        string Description { get; }
        int MaxLevel { get; }
        IAbility[] Abilities { get; }
    }

    public class Skill : ISkill
    {
        public Skill(int id, string name, string description, IAbility[] abilities)
        {
            Id = id;
            Name = name;
            Description = description;
            MaxLevel = abilities.Max(x => x.Level);
            Abilities = abilities;
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int MaxLevel { get; }
        public IAbility[] Abilities { get; }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return $"{Name} ({Description})";
        }
    }
}
