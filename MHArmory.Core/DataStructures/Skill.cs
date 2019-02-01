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
        Dictionary<string, string> Description { get; }
    }

    public class AbilityEqualityComparer : IEqualityComparer<IAbility>
    {
        public static readonly IEqualityComparer<IAbility> Default = new AbilityEqualityComparer();

        public bool Equals(IAbility x, IAbility y)
        {
            if (x != null && y != null)
                return x.Id == y.Id;

            return false;
        }

        public int GetHashCode(IAbility obj)
        {
            if (obj != null)
                return obj.Id;

            return 0;
        }
    }

    public class Ability : IAbility
    {
        public Ability(ISkill skill, int level, Dictionary<string, string> description)
        {
            Skill = skill;
            Level = level;
            Description = description;

            hashCode = $"{Localization.GetDefault(Skill.Name)}|{Level}".GetHashCode();
        }

        private int hashCode;

        public int Id { get; private set; }
        public ISkill Skill { get; }
        public int Level { get; }
        public Dictionary<string, string> Description { get; private set; }

        public void Update(int id, Dictionary<string, string> description)
        {
            Id = id;
            Description = description;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override string ToString()
        {
            return $"{Localization.GetDefault(Skill.Name)} level {Level} ({Localization.GetDefault(Description)})";
        }
    }

    public interface ISkill
    {
        int Id { get; }
        Dictionary<string, string> Name { get; }
        Dictionary<string, string> Description { get; }
        int MaxLevel { get; }
        IAbility[] Abilities { get; }
        string[] Categories { get; }
    }

    public class Skill : ISkill
    {
        public Skill(int id, Dictionary<string, string> name, Dictionary<string, string> description, IAbility[] abilities, string[] categories)
        {
            Id = id;
            Name = name;
            Description = description;
            MaxLevel = abilities.Max(x => x.Level);
            Abilities = abilities;
            Categories = categories;
        }

        public int Id { get; }
        public Dictionary<string, string> Name { get; }
        public Dictionary<string, string> Description { get; }
        public int MaxLevel { get; }
        public IAbility[] Abilities { get; }
        public string[] Categories { get; }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return $"{Localization.GetDefault(Name)} ({Localization.GetDefault(Description)})";
        }
    }
}
