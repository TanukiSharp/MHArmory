﻿using System;

namespace MHArmory
{
    public enum SearchResultSortCriteria
    {
        BaseDefense,
        MaxUnaugmentedDefense,
        MaxAugmentedDefense,
        TotalRarity,
        HighestRarity,
        AverageRarity,
        SpareSlotCount,
        SpareSlotSizeSquare,
        SpareSlotSizeCube,
        FireResistance,
        WaterResistance,
        ThunderResistance,
        IceResistance,
        DragonResistance,
    }

    public abstract class SearchResultSortCriteriaDataSource : EnumDataSource<SearchResultSortCriteria>
    {
    }
}