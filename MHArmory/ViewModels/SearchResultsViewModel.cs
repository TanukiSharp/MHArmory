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
    public class SearchResultsViewModel : ViewModelBase
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
        private IEnumerable<ArmorSetViewModel> rawFoundArmorSets;

        private IEnumerable<KeyValuePair<ArmorSetViewModel, IEnumerable<ArmorSetViewModel>>> groupedFoundArmorSets;
        public IEnumerable<KeyValuePair<ArmorSetViewModel, IEnumerable<ArmorSetViewModel>>> GroupedFoundArmorSets
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
                    OnFoundArmorSets();
            }
        }

        public ICommand SearchArmorSetsCommand { get; }
        public ICommand CancelArmorSetsSearchCommand { get; }

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
        }

        public void Initialize()
        {
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

            if (UngroupedFoundArmorSets == null || currentGroupingFlags == SearchResultsGrouping.None)
                return;

            var sw = Stopwatch.StartNew();

            IEnumerable<KeyValuePair<ArmorSetViewModel, IEnumerable<ArmorSetViewModel>>> groups = SearchResultGrouper.Default
                .GroupBy(UngroupedFoundArmorSets, currentGroupingFlags)
                .Select(g => new KeyValuePair<ArmorSetViewModel, IEnumerable<ArmorSetViewModel>>(g.FirstOrDefault(), g))
                .ToList();

            sw.Stop();
            Console.WriteLine($"Grouping took {sw.ElapsedMilliseconds} ms");

            GroupedFoundArmorSets = groups;
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

        private void OnFoundArmorSets()
        {
            //SearchResultsGrouping grouping =
            //    SearchResultsGrouping.RequiredDecorations |
            //    SearchResultsGrouping.Defense |
            //    SearchResultsGrouping.SpareSlots |
            //    SearchResultsGrouping.AdditionalSKills |
            //    SearchResultsGrouping.Resistances |
            //    SearchResultsGrouping.None;

            //var sw = Stopwatch.StartNew();

            //IEnumerable<KeyValuePair<ArmorSetViewModel, IEnumerable<ArmorSetViewModel>>> groups = SearchResultGrouper.Default
            //    .GroupBy(foundArmorSets, grouping)
            //    .Select(g => new KeyValuePair<ArmorSetViewModel, IEnumerable<ArmorSetViewModel>>(g.FirstOrDefault(), g))
            //    .ToList();

            //sw.Stop();
            //Console.WriteLine($"Grouping took {sw.ElapsedMilliseconds} ms");

            //GroupedFoundArmorSets = groups;
        }
    }
}
