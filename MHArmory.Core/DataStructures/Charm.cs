using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public interface ICharmLevel : IEquipment
    {
        ICharm Charm { get; }
        int Level { get; }

        void UpdateCharm(ICharm charm);
    }

    public class CharmLevel : ICharmLevel
    {
        public CharmLevel(int id, int level, Dictionary<string, string> name, int rarity, int[] slots, IAbility[] abilities, IEvent evt, ICraftMaterial[] craftMaterials)
        {
            Id = id;
            Level = level;
            Type = EquipmentType.Charm;
            Name = name;
            Rarity = rarity;
            Slots = slots;
            Abilities = abilities;
            Event = evt;
            CraftMaterials = craftMaterials;
        }

        public ICharm Charm { get; private set; }
        public int Id { get; }
        public int Level { get; }
        public EquipmentType Type { get; }
        public Dictionary<string, string> Name { get; }
        public int Rarity { get; }
        public int[] Slots { get; }
        public IAbility[] Abilities { get; }
        public IEvent Event { get; }
        public ICraftMaterial[] CraftMaterials { get; }

        public void UpdateCharm(ICharm charm)
        {
            Charm = charm;
        }

        public override string ToString()
        {
            return Localization.GetDefault(Name);
        }
    }

    public interface ICharm
    {
        int Id { get; }
        Dictionary<string, string> Name { get; }
        ICharmLevel[] Levels { get; }
    }

    public class Charm : ICharm
    {
        public Charm(int id, Dictionary<string, string> name, ICharmLevel[] levels)
        {
            Id = id;
            Name = name;
            Levels = levels;

            foreach (ICharmLevel charmLevel in levels)
                charmLevel.UpdateCharm(this);
        }

        public int Id { get; }
        public Dictionary<string, string> Name { get; }
        public ICharmLevel[] Levels { get; }

        public override string ToString()
        {
            if (Name != null)
                return $"{Localization.GetDefault(Name)} ({Levels.Length})";

            return base.ToString();
        }
    }
}
