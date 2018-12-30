using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Search.OpenCL
{
    class LengthConstants
    {
        public const int SlotArrayLength = SearchLimits.MaxJewelSize + 1; // +1 because it's also counting no-slot into index 0
        public const int SlottableEquipmentCount = SearchLimits.EquipmentTypes + 1; // +1 because you can slot the weapon too
        public const int DecorationsPerResult = SlottableEquipmentCount * SearchLimits.SlotsPerEquipment;
        public const int DecorationLength = sizeof(ushort) + sizeof(byte);
        public const int DecorationListLength = DecorationsPerResult * DecorationLength;
        public const int SingleResultLength = sizeof(ushort) * SearchLimits.EquipmentTypes + sizeof(byte) + DecorationListLength + SlotArrayLength;
        public const int ResultLength = SearchLimits.ResultCount * SingleResultLength;
    }
}
