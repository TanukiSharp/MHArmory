using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.OpenCL
{
    public class SearchIDMaps
    {
        public IDictionary<Tuple<int, EquipmentType>, byte> EquipmentIdMap { get; set; }
        public IDictionary<int, byte> JewelIdMap { get; set; }
        public IDictionary<int, byte> SetIdMap { get; set; }
        public IDictionary<int, byte> SkillIdMap { get; set; }
    }

    public class SerializedSearchParameters
    {
        public uint Combinations { get; set; }
        public SearchIDMaps SearchIDMaps { get; set; }
        public byte[] Header { get; set; }
        public byte[] Equipment { get; set; }
        public byte[] Decorations { get; set; }
        public byte[] DesiredSkills { get; set; }
    }
}
