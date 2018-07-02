using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public interface ICharmLevel : IEquipment
    {
        int Level { get; }
    }

    public interface ICharm
    {
        string Name { get; }
        ICharmLevel[] Levels { get; }
    }
}
