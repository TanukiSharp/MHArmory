using System;

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
        SlotCount,
        SlotSizeSquare,
        SlotSizeCube,
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
