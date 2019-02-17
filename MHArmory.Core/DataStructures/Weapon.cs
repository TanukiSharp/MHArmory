using System;
using System.Collections.Generic;

namespace MHArmory.Core.DataStructures
{
    public enum WeaponType
    {
        GreatSword,
        LongSword,
        SwordAndShield,
        DualBlades,
        Hammer,
        HuntingHorn,
        Lance,
        Gunlance,
        SwitchAxe,
        ChargeBlade,
        InsectGlaive,
        LightBowgun,
        HeavyBowgun,
        Bow
    }

    public enum ElementType
    {
        Fire,
        Water,
        Thunder,
        Ice,
        Dragon,
        Poison,
        Sleep,
        Paralysis,
        Blast
    }

    public class Weapon : IEquipment
    {
        public int Id { get; }
        public EquipmentType Type { get; } = EquipmentType.Weapon;
        public WeaponType WeaponType { get; }
        public Dictionary<string, string> Name { get; }
        public int Rarity { get; }
        public int[] Slots { get; }
        public IAbility[] Abilities { get; }
        public IEvent Event { get; }
        public ICraftMaterial[] CraftMaterials { get; }

        public Weapon(int id, WeaponType weaponType, int[] slots, IAbility[] abilities, IEvent evt)
        {
            Id = id;
            WeaponType = weaponType;
            Slots = slots;
            Abilities = abilities;
            Event = evt;
        }

        public Weapon(int id, Dictionary<string, string> name, WeaponType weaponType, int rarity, int[] slots, IAbility[] abilities, IEvent evt)
        {
            Id = id;
            Name = name;
            WeaponType = weaponType;
            Rarity = rarity;
            Slots = slots;
            Abilities = abilities;
            Event = evt;
        }
    }
}
