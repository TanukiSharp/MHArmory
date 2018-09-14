using System;

namespace MHArmory.Core.DataStructures
{
    public class Weapon : IEquipment
    {
        public int Id { get; }
        public EquipmentType Type { get; } = EquipmentType.Weapon;
        public string Name { get; }
        public int Rarity { get; }
        public int[] Slots { get; }
        public IAbility[] Abilities { get; }
        public IEvent Event { get; }

        public Weapon(int id, int[] slots, IAbility[] abilities, IEvent evt)
        {
            Id = id;
            Slots = slots;
            Abilities = abilities;
            Event = evt;
        }

        public Weapon(int id, string name, int rarity, int[] slots, IAbility[] abilities, IEvent evt)
        {
            Id = id;
            Name = name;
            Rarity = rarity;
            Slots = slots;
            Abilities = abilities;
            Event = evt;
        }
    }
}
