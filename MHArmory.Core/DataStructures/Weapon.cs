using System;

namespace MHArmory.Core.DataStructures
{
    public class Weapon : IEquipment
    {
        public EquipmentType Type => EquipmentType.Weapon;
        public string Name { get; }
        public int Rarity { get; }
        public int[] Slots { get; }
        public IAbility[] Abilities { get; }

        public Weapon(int[] slots, IAbility[] abilities)
        {
            Slots = slots;
            Abilities = abilities;
        }

        public Weapon(string name, int rarity, int[] slots, IAbility[] abilities)
        {
            Name = name;
            Rarity = rarity;
            Slots = slots;
            Abilities = abilities;
        }
    }
}
