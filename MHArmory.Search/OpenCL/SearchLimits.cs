using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Search.OpenCL
{
    class SearchLimits
    {
        public const int ArmorSkillCount = 3;
        public const int SetSkillCount = 3;
        public const int DesiredSkillCount = 254; // not 255 because 0xFF is mapped to "no skill"
        public const int DecorationCount = 255;   // not 255 because 0xFF is mapped to "no decoration"

        public const int ResultCount = 128;
    }
}
