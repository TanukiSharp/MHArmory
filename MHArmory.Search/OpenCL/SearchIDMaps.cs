using System;
using System.Collections.Generic;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.OpenCL
{
    internal class SearchIDMaps
    {
        public IDictionary<(int, EquipmentType), byte> EquipmentIdMap { get; set; }
        public IDictionary<int, byte> JewelIdMap { get; set; }
        public IDictionary<int, byte> SetIdMap { get; set; }
        public IDictionary<int, byte> SkillIdMap { get; set; }
    }
}
