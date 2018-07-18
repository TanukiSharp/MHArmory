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
}
