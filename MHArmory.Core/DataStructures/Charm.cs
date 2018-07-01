using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public interface ICharmLevel
    {
        string Name { get; }
        int Level { get; }
        int Rarity { get; }
        int[] Slots { get; }
        IAbility[] Abilities { get; }
    }

    public interface ICharm
    {
        string Name { get; }
        ICharmLevel[] Levels { get; }
    }
}
