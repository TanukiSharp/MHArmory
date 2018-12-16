using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Search.OpenCL
{
    class SearchLimits
    {
        public const int ArmorSkillCount = 3;
        public const int SetSkillCount = 3;
        public const int DesiredSkillCount = 0xFF; // Must be exactly 0xFF, because 0xFF is mapped to "no skill"
        public const int DecorationCount = 0xFF;   // Must be exactly 0xFF, because 0xFF is mapped to "no decoration"

        public const int ResultCount = 128;
    }
}
