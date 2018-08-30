using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MHArmory.ValueConverters
{
    public class SortCriteriaNameValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SearchResultSortCriteria s)
            {
                switch (s)
                {
                    case SearchResultSortCriteria.BaseDefense: return "Base defense";
                    case SearchResultSortCriteria.MaxUnaugmentedDefense: return "Maximum defense";
                    case SearchResultSortCriteria.MaxAugmentedDefense: return "Maximum augmented defense";
                    case SearchResultSortCriteria.TotalRarity: return "Total rarity (sum of rarity of armor pieces)";
                    case SearchResultSortCriteria.HighestRarity: return "Highest rarity (highest rarity among armor pieces)";
                    case SearchResultSortCriteria.AverageRarity: return "Average rarity (average rarity of armor pieces)";
                    case SearchResultSortCriteria.SpareSlotCount: return "Spare slot count (no distinction between level 1, 2 and 3)";
                    case SearchResultSortCriteria.SpareSlotSizeSquare: return "Spare slot size (square value)";
                    case SearchResultSortCriteria.SpareSlotSizeCube: return "Spare slot size (cube value)";
                    case SearchResultSortCriteria.FireResistance: return "Fire resistance";
                    case SearchResultSortCriteria.WaterResistance: return "Water resistance";
                    case SearchResultSortCriteria.ThunderResistance: return "Thunder resistance";
                    case SearchResultSortCriteria.IceResistance: return "Ice resistance";
                    case SearchResultSortCriteria.DragonResistance: return "Dragon resistance";
                    case SearchResultSortCriteria.AdditionalSkillsCount: return "Additional skills count";
                    case SearchResultSortCriteria.AdditionalSkillsTotalLevel: return "Additional skills total level";
                }
            }

            return "(unknown)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
