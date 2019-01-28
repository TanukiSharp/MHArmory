using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using MHArmory.Core.WPF;
using MHArmory.Search.Contracts;

namespace MHArmory.ViewModels
{
    public class SearchResultsViewModel : ViewModelBase, Core.WPF.Behaviors.ISelectionChangedViewNotifier
    {
        private readonly RootViewModel root;

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

        public EnumFlagViewModel<SearchResultsGrouping>[] GroupFlags { get; }

        private SearchResultsGrouping currentGroupingFlags = SearchResultsGrouping.None;
            //SearchResultsGrouping.RequiredDecorations |
            //SearchResultsGrouping.Defense |
            //SearchResultsGrouping.SpareSlots |
            //SearchResultsGrouping.AdditionalSKills |
            //SearchResultsGrouping.Resistances;

        private IEnumerable<ArmorSetViewModel> rawFoundArmorSets;

        private bool isMasterViewUnlocked;
        public bool IsMasterViewUnlocked
        {
            get { return isMasterViewUnlocked; }
            private set { SetValue(ref isMasterViewUnlocked, value); }
        }

        private GroupedArmorSetHeaderViewModel selectedGroup;
        public GroupedArmorSetHeaderViewModel SelectedGroup
        {
            get { return selectedGroup; }
            set
            {
                GroupedArmorSetHeaderViewModel oldGroup = selectedGroup;

                if (SetValue(ref selectedGroup, value))
                {
                    if (oldGroup != null)
                        oldGroup.IsSelected = false;

                    if (selectedGroup != null)
                    {
                        selectedGroup.IsSelected = true;
                        IsMasterViewUnlocked = false;
                        SelectionChangedHandler?.Invoke(selectedGroup);
                    }
                }
                else
                {
                    IsMasterViewUnlocked = true;

                    if (SelectedGroup != null) // This if avoids stack overflow exception.
                        SelectedGroup = null;
                }
            }
        }

        private IList<GroupedArmorSetHeaderViewModel> groupedFoundArmorSets;
        public IList<GroupedArmorSetHeaderViewModel> GroupedFoundArmorSets
        {
            get { return groupedFoundArmorSets; }
            private set { SetValue(ref groupedFoundArmorSets, value); }
        }

        private IEnumerable<ArmorSetViewModel> ungroupedFoundArmorSets;
        public IEnumerable<ArmorSetViewModel> UngroupedFoundArmorSets
        {
            get { return ungroupedFoundArmorSets; }
            private set
            {
                if (SetValue(ref ungroupedFoundArmorSets, value))
                    UpdateGroups();
            }
        }

        private bool isGroupedViewMode;
        public bool IsGroupedViewMode
        {
            get { return isGroupedViewMode; }
            private set { SetValue(ref isGroupedViewMode, value); }
        }

        private bool hasGroupedResults;
        public bool HasGroupedResults
        {
            get { return hasGroupedResults; }
            private set { SetValue(ref hasGroupedResults, value); }
        }

        public ICommand SearchArmorSetsCommand { get; }
        public ICommand CancelArmorSetsSearchCommand { get; }

        public Action<object> SelectionChangedHandler { get; set; }

        public SearchResultsViewModel(RootViewModel root)
        {
            this.root = root;

            SearchArmorSetsCommand = new AnonymousCommand(SearchArmorSets);
            CancelArmorSetsSearchCommand = new AnonymousCommand(OnCancelArmorSetsSearch);

            GroupFlags = new EnumFlagViewModel<SearchResultsGrouping>[]
            {
                CreateEnumFlag("Decorations", SearchResultsGrouping.RequiredDecorations),
                CreateEnumFlag("Defense", SearchResultsGrouping.Defense),
                CreateEnumFlag("Spare slots", SearchResultsGrouping.SpareSlots),
                CreateEnumFlag("Additional skills", SearchResultsGrouping.AdditionalSKills),
                CreateEnumFlag("Resistances", SearchResultsGrouping.Resistances),
            };

            IsGroupedViewMode = currentGroupingFlags != SearchResultsGrouping.None;
        }

        public void ResetResults()
        {
            rawFoundArmorSets = null;
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

            IsGroupedViewMode = currentGroupingFlags != SearchResultsGrouping.None;

            UpdateGroups();
        }

        private void UpdateGroups()
        {
            if (UngroupedFoundArmorSets == null || currentGroupingFlags == SearchResultsGrouping.None)
                return;

            IList<GroupedArmorSetHeaderViewModel> groups = SearchResultGrouper.Default
                .GroupBy(UngroupedFoundArmorSets, currentGroupingFlags)
                .Where(x => x.Any())
                .Select(g => new GroupedArmorSetHeaderViewModel(this, g.ToList()))
                .ToList();

            GroupedFoundArmorSets = groups;
            HasGroupedResults = groups.Count > 0;

            if (HasGroupedResults)
                SelectedGroup = groups[0];
            else
                SelectedGroup = null;

            SelectionChangedHandler?.Invoke(SelectedGroup);
        }

        public void ApplySorting(bool force, int limit = 200)
        {
            if (rawFoundArmorSets == null)
                return;

            IEnumerable<ArmorSetViewModel> result = rawFoundArmorSets;

            if (root.SearchResultProcessing.ApplySort(ref result, force, limit))
                UngroupedFoundArmorSets = result;
        }

        public void ApplySorting(SearchResultProcessingContainerViewModel container, bool force, int limit = 200)
        {
            if (rawFoundArmorSets == null)
                return;

            IEnumerable<ArmorSetViewModel> result = rawFoundArmorSets;

            if (root.SearchResultProcessing.ApplySort(ref result, container, force, limit))
                UngroupedFoundArmorSets = result;
        }

        private CancellationTokenSource searchCancellationTokenSource;
        private Task previousSearchTask;

        public async void SearchArmorSets()
        {
            try
            {
                await SearchArmorSetsInternal();
            }
            catch (TaskCanceledException)
            {
            }
        }

        private async void OnCancelArmorSetsSearch()
        {
            await CancelArmorSetsSearch();
        }

        public async Task CancelArmorSetsSearch()
        {
            if (searchCancellationTokenSource != null)
            {
                if (searchCancellationTokenSource.IsCancellationRequested)
                    return;

                searchCancellationTokenSource.Cancel();

                if (previousSearchTask != null)
                {
                    try
                    {
                        await previousSearchTask;
                    }
                    catch
                    {
                    }
                }
            }
        }

        public async Task SearchArmorSetsInternal()
        {
            await CancelArmorSetsSearch();

            searchCancellationTokenSource = new CancellationTokenSource();
            previousSearchTask = SearchArmorSetsInternal(searchCancellationTokenSource.Token);

            root.SearchProgression = 0.0;
            root.IsSearching = true;

            try
            {
                await previousSearchTask;
            }
            finally
            {
                root.IsSearching = false;

                previousSearchTask = null;

                if (searchCancellationTokenSource != null)
                {
                    searchCancellationTokenSource.Cancel();
                    searchCancellationTokenSource = null;
                }
            }
        }

        private async Task SearchArmorSetsInternal(CancellationToken cancellationToken)
        {
            ISolverData solverData = root.GetSelectedSolverData();

            if (solverData == null)
                return;

            var sw = Stopwatch.StartNew();

            IList<ArmorSetSearchResult> result = await root.GetSelectedSolver().SearchArmorSets(solverData, cancellationToken);

            sw.Stop();

            SearchMetrics metrics = root.SearchMetrics;

            metrics.TimeElapsed = (int)sw.ElapsedMilliseconds;
            metrics.MatchingResults = result?.Count ?? 0;

            await Dispatcher.Yield(DispatcherPriority.SystemIdle);

            root.SearchMetrics = null;
            root.SearchMetrics = metrics;

            IsDataLoading = true;
            IsDataLoaded = false;

            await Dispatcher.Yield(DispatcherPriority.SystemIdle);

            try
            {
                if (solverData == null)
                {
                    rawFoundArmorSets = null;
                    return;
                }

                if (result != null)
                {
                    rawFoundArmorSets = result.Where(x => x.IsMatch).Select(x => new ArmorSetViewModel(
                        root,
                        solverData,
                        x.ArmorPieces,
                        x.Charm,
                        x.Jewels.Select(j => new ArmorSetJewelViewModel(j.Jewel, j.Count)).ToList(),
                        x.SpareSlots
                    ));

                    ApplySorting(true);
                }
            }
            finally
            {
                IsDataLoading = false;
                IsDataLoaded = true;
            }
        }
    }
}
