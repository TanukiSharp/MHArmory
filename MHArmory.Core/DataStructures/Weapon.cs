using System;

namespace MHArmory.Core.DataStructures
{
    public class Weapon : IEquipment
    {
        public int Id { get; }
        public EquipmentType Type => EquipmentType.Weapon;
        public string Name { get; }
        public int Rarity { get; }
        public int[] Slots { get; }
        public IAbility[] Abilities { get; }

        public Weapon(int id, int[] slots, IAbility[] abilities)
        {
            Id = id;
            Slots = slots;
            Abilities = abilities;
        }

        public Weapon(int id, string name, int rarity, int[] slots, IAbility[] abilities)
        {
            Id = id;
            Name = name;
            Rarity = rarity;
            Slots = slots;
            Abilities = abilities;
        }
    }
}
