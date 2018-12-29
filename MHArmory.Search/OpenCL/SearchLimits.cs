using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Search.OpenCL
{
    internal class SearchLimits
    {
        // WARNING:
        // Changing some these parameters may result in incorrect search behavior,
        // because some assumptions in the implementation were made.
        // For example: 0xFF means "no thingie" in some cases

        public const int ResultCount = 128;         // Should be safe to change

        public const int TotalEquipments = 0x100;   // One byte for mapped ID
        public const int TotalJewels = 0x100;       // One byte for mapped ID

        public const int MaxJewelSize = 3;
        public const int SlotsPerEquipment = 3;
        public const int SkillsPerEquipment = 3;    // Should be safe to change
        public const int SetSkillsPerEquipment = 3; // Should be safe to change
        public const int EquipmentTypes = 6;        // Excluding weapon, including charm

        public const int MaxDesiredSkills = 0x80;    // DO NOT CHANGE - Must be exactly 0xFF, because 0xFF is mapped to "no skill" in the implementation
        public const int MaxSetSkills = 0x40;      // DO NOT CHANGE - Must be exactly 0xFF, because 0xFF is mapped to "no set part" in the implementation
    }
}
