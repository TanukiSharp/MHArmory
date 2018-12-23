using System;
using System.Collections.Generic;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Contracts
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
        public IList<IArmorPiece> ArmorPieces;
        public ICharmLevel Charm;
        public IList<ArmorSetJewelResult> Jewels;
        public int[] SpareSlots;
    }
}
