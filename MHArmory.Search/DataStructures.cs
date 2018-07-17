using System;
using System.Collections.Generic;
using System.Text;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search
{
    public struct ArmorSetJewelResult
    {
        public IJewel Jewel;
        public int Count;
    }

    public struct ArmorSetSearchResult
    {
        public static readonly ArmorSetSearchResult NoMatch = new ArmorSetSearchResult { IsMatch = false };

        public bool IsMatch;
        public bool IsOptimal;
        public IList<IArmorPiece> ArmorPieces;
        public ICharmLevel Charm;
        public IList<ArmorSetJewelResult> Jewels;
    }
}
