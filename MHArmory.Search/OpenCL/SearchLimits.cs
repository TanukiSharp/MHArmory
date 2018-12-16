using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Search.OpenCL
{
    class SearchLimits
    {
        public const int EquipmentPieces = 0x100;  // Byte for mapped ID

        public const int ArmorSkillCount = 3;
        public const int SetSkillCount = 3;

        public const int SkillCount = 0xFF;       // Must be exactly 0xFF, because 0xFF is mapped to "no skill" in the implementation
        public const int DecorationCount = 0xFF;  // Must be exactly 0xFF, because 0xFF is mapped to "no decoration" in the implementation

        public const int ResultCount = 128;
    }
}
