using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public interface IJewel : IHasAbilities
    {
        int Id { get; }
        string Name { get; }
        int Rarity { get; }
        int SlotSize { get; }
    }

    public class Jewel : IJewel
    {
        public Jewel(int id, string name, int rarity, int slotSize, IAbility[] abilities)
        {
            Id = id;
            Name = name;
            Rarity = rarity;
            SlotSize = slotSize;
            Abilities = abilities;
        }

        public int Id { get; }
        public string Name { get; }
        public int Rarity { get; }
        public int SlotSize { get; }
        public IAbility[] Abilities { get; }
    }
}
