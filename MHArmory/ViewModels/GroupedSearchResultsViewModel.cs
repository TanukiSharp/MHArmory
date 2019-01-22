using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public class EnumFlagViewModel<T> : ViewModelBase
    {
        public string Name { get; }

        private bool isSet;
        public bool IsSet
        {
            get { return isSet; }
            set
            {
                if (SetValue(ref isSet, value))
                    notifyChanged(value, enumValue);
            }
        }

        private readonly T enumValue;
        private readonly Action<bool, T> notifyChanged;

        public EnumFlagViewModel(string name, bool initialValue, T enumValue, Action<bool, T> onChanged)
        {
            Name = name;
            IsSet = initialValue;
            this.enumValue = enumValue;
            notifyChanged = onChanged;
        }
    }

    public class GroupedSearchResultsViewModel : ViewModelBase
    {
        public EnumFlagViewModel<SearchResultsGrouping>[] Flags { get; }

        private IEnumerable<KeyValuePair<ArmorSetViewModel, IEnumerable<ArmorSetViewModel>>> groupedFoundArmorSets;
        public IEnumerable<KeyValuePair<ArmorSetViewModel, IEnumerable<ArmorSetViewModel>>> GroupedFoundArmorSets
        {
            get { return groupedFoundArmorSets; }
            private set { SetValue(ref groupedFoundArmorSets, value); }
        }

        private SearchResultsGrouping currentGroupingFlags = SearchResultsGrouping.None;
        private readonly RootViewModel root;

        public GroupedSearchResultsViewModel(RootViewModel root)
        {
            this.root = root;

            Flags = new EnumFlagViewModel<SearchResultsGrouping>[]
            {
                CreateEnumFlag("Decorations", SearchResultsGrouping.RequiredDecorations),
                CreateEnumFlag("Defense", SearchResultsGrouping.Defense),
                CreateEnumFlag("Spare slots", SearchResultsGrouping.SpareSlots),
                CreateEnumFlag("Additional skills", SearchResultsGrouping.AdditionalSKills),
                CreateEnumFlag("Resistances", SearchResultsGrouping.Resistances),
            };
        }

        private EnumFlagViewModel<SearchResultsGrouping> CreateEnumFlag(string name, SearchResultsGrouping value)
        {
            return new EnumFlagViewModel<SearchResultsGrouping>(
                name,
                (currentGroupingFlags & value) == value,
                value,
                OnFlagChanged
            );
        }

        private void OnFlagChanged(bool isSet, SearchResultsGrouping value)
        {
            if (isSet)
                currentGroupingFlags |= value;
            else
                currentGroupingFlags &= ~value;

            if (root.FoundArmorSets == null)
                return;

            var sw = Stopwatch.StartNew();

            IEnumerable<KeyValuePair<ArmorSetViewModel, IEnumerable<ArmorSetViewModel>>> groups = SearchResultGrouper.Default
                .GroupBy(root.FoundArmorSets, currentGroupingFlags)
                .Select(g => new KeyValuePair<ArmorSetViewModel, IEnumerable<ArmorSetViewModel>>(g.FirstOrDefault(), g))
                .ToList();

            sw.Stop();
            Console.WriteLine($"Grouping took {sw.ElapsedMilliseconds} ms");

            GroupedFoundArmorSets = groups;
        }
    }
}
