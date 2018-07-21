using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public interface ICharmLevel : IEquipment
    {
        int Level { get; }
    }

    public class CharmLevel : ICharmLevel
    {
        public CharmLevel(int level, string name, int rarity, int[] slots, IAbility[] abilities)
        {
            Level = level;
            Type = EquipmentType.Charm;
            Name = name;
            Rarity = rarity;
            Slots = slots;
            Abilities = abilities;
        }

        public int Level { get; }
        public EquipmentType Type { get; }
        public string Name { get; }
        public int Rarity { get; }
        public int[] Slots { get; }
        public IAbility[] Abilities { get; }
    }

    public interface ICharm
    {
        string Name { get; }
        ICharmLevel[] Levels { get; }
    }

    public class Charm : ICharm
    {
        public Charm(string name, ICharmLevel[] levels)
        {
            Name = name;
            Levels = levels;
        }

        public string Name { get; }
        public ICharmLevel[] Levels { get; }

        public override string ToString()
        {
            return $"{Name} ({Levels.Length})";
        }
    }
}
