using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.AthenaAssDataSource.DataStructures
{
    internal class JewelPrimitive
    {
        internal string Name = null;
        [Name("Slot Level")]
        internal int SlotSize = 0;
        internal int Rarity = 0;
        internal string Skill = null;
    }
}
