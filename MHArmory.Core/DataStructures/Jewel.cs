using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public interface IJewel : IHasAbilities
    {
        string Name { get; }
        int Rarity { get; }
        int SlotSize { get; }
    }

    public class Jewel : IJewel
    {
        public Jewel(string name, int rarity, int slotSize, IAbility[] abilities)
        {
            Name = name;
            Rarity = rarity;
            SlotSize = slotSize;
            Abilities = abilities;
        }

        public string Name { get; }
        public int Rarity { get; }
        public int SlotSize { get; }
        public IAbility[] Abilities { get; }
    }
}
