using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Search;
using MHArmory.Searching;

namespace MHArmory.ViewModels
{
    public class RootViewModel : ViewModelBase
    {
        public ICommand OpenSkillSelectorCommand { get; }
        public ICommand SearchArmorSetsCommand { get; }

        private SolverData solverData;
        private Solver solver;

        private bool isDataLoading = true;
        public bool IsDataLoading
        {
            get { return isDataLoading; }
            set { SetValue(ref isDataLoading, value); }
        }

        private bool isDataLoaded;
        public bool IsDataLoaded
        {
            get { return isDataLoaded; }
            set { SetValue(ref isDataLoaded, value); }
        }

        private IEnumerable<AbilityViewModel> selectedAbilities;
        public IEnumerable<AbilityViewModel> SelectedAbilities
        {
            get { return selectedAbilities; }
            set { SetValue(ref selectedAbilities, value); }
        }

        private IEnumerable<ArmorSetViewModel> foundArmorSets;
        public IEnumerable<ArmorSetViewModel> FoundArmorSets
        {
            get { return foundArmorSets; }
            set { SetValue(ref foundArmorSets, value); }
        }

        private bool isSearching;
        public bool IsSearching
        {
            get { return isSearching; }
            private set { SetValue(ref isSearching, value); }
        }

        private bool isAutoSearch;
        public bool IsAutoSearch
        {
            get { return isAutoSearch; }
            set { SetValue(ref isAutoSearch, value); }
        }

        public RootViewModel()
        {
            OpenSkillSelectorCommand = new AnonymousCommand(OpenSkillSelector);
            SearchArmorSetsCommand = new AnonymousCommand(SearchArmorSets);
        }

        public void Initialize()
        {
            //IDictionary<int, IList<ICharm>> skillsToCharmsMap = await GlobalData.Instance.GetSkillsToCharmsMap();
            //IDictionary<int, IList<IJewel>> skillsToJewelsMap = await GlobalData.Instance.GetSkillsToJewelsMap();



            IList <IArmorPiece> allHeads = GlobalData.Instance.Heads;
        }

        private void OpenSkillSelector(object parameter)
        {
            RoutedCommands.OpenSkillsSelector.Execute(null);
        }

        public async void SearchArmorSets()
        {
            var desiredAbilities = SelectedAbilities
                .Where(x => x.IsChecked)
                .Select(x => x.Ability)
                .ToList();

            solverData = new SolverData(
                GlobalData.Instance.Heads,
                GlobalData.Instance.Chests,
                GlobalData.Instance.Gloves,
                GlobalData.Instance.Waists,
                GlobalData.Instance.Legs,
                GlobalData.Instance.Charms,
                GlobalData.Instance.Jewels,
                desiredAbilities
            );

            solverData.Done();

            solver = new Solver(solverData);

            solver.DebugData += Solver_DebugData;
            solver.SearchingChanged += Solver_SearchingChanged;

            IList<ArmorSetSearchResult> result = await solver.SearchArmorSets();

            if (result == null)
                FoundArmorSets = null;
            else
            {
                FoundArmorSets = result.Where(x => x.IsMatch).Select(x => new ArmorSetViewModel
                {
                    ArmorPieces = x.ArmorPieces,
                    Charm = x.Charm,
                    Jewels = x.Jewels.Select(j => new ArmorSetJewelViewModel { Jewel = j.Jewel, Count = j.Count }).ToList()
                });
            }

            //if (IsSearching)
            //    return;

            //IsSearching = true;

            //try
            //{
            //await SearchArmorSetsInternal();
            //}
            //finally
            //{
            //    IsSearching = false;
            //}

            //solverData.

            solver.SearchingChanged -= Solver_SearchingChanged;
            solver.DebugData -= Solver_DebugData;
        }

        private void Solver_SearchingChanged(bool isSearching)
        {
            IsSearching = isSearching;
        }

        private void Solver_DebugData(string debugData)
        {
            SearchResult = debugData;
        }

        private MaximizedSearchCriteria[] sortCriterias = new MaximizedSearchCriteria[]
        {
            MaximizedSearchCriteria.BaseDefense,
            MaximizedSearchCriteria.DragonResistance,
            MaximizedSearchCriteria.SlotSize
        };




        private class OrderedEnumerable<T> : IOrderedEnumerable<T>
        {
            private readonly IEnumerable<T> source;

            public OrderedEnumerable(IEnumerable<T> source)
            {
                this.source = source;
            }

            public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
            {
                return this;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return source.GetEnumerator();
            }
        }

        // values bellow are max slot size (3) to the power of the slot index (0-based)
        private static readonly int[] slotSizeSortWeight = new[] { 9, 3, 1 };

        private IEnumerable<IArmorPiece> CreateArmorPieceSorter(IEnumerable<IArmorPiece> items, IEnumerable<MaximizedSearchCriteria> sortCriterias)
        {
            if (items.Any() == false)
                return items;

            //IOrderedEnumerable<IArmorPiece> result = items.OrderBy(x => 1); // wasting a bit of CPU cycles for productivity purpose :(
            IOrderedEnumerable<IArmorPiece> result = new OrderedEnumerable<IArmorPiece>(items);

            foreach (MaximizedSearchCriteria sortCriteria in sortCriterias)
            {
                switch (sortCriteria)
                {
                    case MaximizedSearchCriteria.BaseDefense:
                        result = result.ThenByDescending(x => x.Defense.Base);
                        break;
                    case MaximizedSearchCriteria.MaxUnaugmentedDefense:
                        result = result.ThenByDescending(x => x.Defense.Max);
                        break;
                    case MaximizedSearchCriteria.MaxAugmentedDefense:
                        result = result.ThenByDescending(x => x.Defense.Augmented);
                        break;
                    case MaximizedSearchCriteria.Rarity:
                        result = result.ThenBy(x => x.Rarity);
                        break;
                    case MaximizedSearchCriteria.SlotCount:
                        result = result.ThenByDescending(x => x.Slots.Length);
                        break;
                    case MaximizedSearchCriteria.SlotSize:
                        result = result.ThenByDescending(x => x.Slots.Select((v, i) => v * slotSizeSortWeight[i]).Sum());
                        break;
                    case MaximizedSearchCriteria.FireResistance:
                        result = result.ThenByDescending(x => x.Resistances.Fire);
                        break;
                    case MaximizedSearchCriteria.WaterResistance:
                        result = result.ThenByDescending(x => x.Resistances.Water);
                        break;
                    case MaximizedSearchCriteria.ThunderResistance:
                        result = result.ThenByDescending(x => x.Resistances.Thunder);
                        break;
                    case MaximizedSearchCriteria.IceResistance:
                        result = result.ThenByDescending(x => x.Resistances.Ice);
                        break;
                    case MaximizedSearchCriteria.DragonResistance:
                        result = result.ThenByDescending(x => x.Resistances.Dragon);
                        break;
                }
            }

            return result;
        }

        private string searchResult;
        public string SearchResult
        {
            get { return searchResult; }
            private set { SetValue(ref searchResult, value); }
        }
    }
}
