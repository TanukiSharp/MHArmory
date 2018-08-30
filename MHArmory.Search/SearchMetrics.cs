using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Search
{
    public class SearchMetrics
    {
        public int Heads { get; set; }
        public int Chests { get; set; }
        public int Gloves { get; set; }
        public int Waists { get; set; }
        public int Legs { get; set; }
        public int Charms { get; set; }
        public int MinSlotSize { get; set; }
        public int MaxSlotSize { get; set; }
        public int CombinationCount { get; private set; }
        public int MatchingResults { get; set; }
        public int TimeElapsed { get; set; }

        public void UpdateCombinationCount()
        {
            CombinationCount =
                Math.Max(Heads, 1) *
                Math.Max(Chests, 1) *
                Math.Max(Gloves, 1) *
                Math.Max(Waists, 1) *
                Math.Max(Legs, 1) *
                Math.Max(Charms, 1);
        }
    }
}
