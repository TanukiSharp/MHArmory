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

namespace MHArmory.ViewModels
{
    public class RootViewModel : ViewModelBase, IDisposable
    {
        public ICommand OpenSkillSelectorCommand { get; }
        public ICommand SearchArmorSetsCommand { get; }
        public ICommand AdvancedSearchCommand { get; }

        public event EventHandler AbilitiesChanged;

        public SolverData SolverData { get; private set; }

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

        internal void NotifyConfigurationLoaded()
        {
            InParameters.NotifyConfigurationLoaded();
        }

        public IEnumerable<ArmorSetViewModel> FoundArmorSets
        {
            get { return foundArmorSets; }
            set { SetValue(ref foundArmorSets, value); }
        }

        public InParametersViewModel InParameters { get; }

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
            AdvancedSearchCommand = new AnonymousCommand(AdvancedSearch);

            InParameters = new InParametersViewModel(this);
        }

        public void Dispose()
        {
            if (loadoutManager != null)
            {
                loadoutManager.LoadoutChanged -= LoadoutManager_LoadoutChanged;
                loadoutManager.ModifiedChanged -= LoadoutManager_ModifiedChanged;
            }
        }

        private LoadoutManager loadoutManager;

        public void SetLoadoutManager(LoadoutManager loadoutManager)
        {
            if (this.loadoutManager != null)
            {
                this.loadoutManager.LoadoutChanged -= LoadoutManager_LoadoutChanged;
                this.loadoutManager.ModifiedChanged -= LoadoutManager_ModifiedChanged;
            }

            this.loadoutManager = loadoutManager;

            if (this.loadoutManager != null)
            {
                this.loadoutManager.LoadoutChanged += LoadoutManager_LoadoutChanged;
                this.loadoutManager.ModifiedChanged += LoadoutManager_ModifiedChanged;
            }
        }

        private string loadoutText;
        public string LoadoutText
        {
            get { return loadoutText; }
            private set { SetValue(ref loadoutText, value); }
        }

        private void UpdateLoadoutText()
        {
            LoadoutText = $"{loadoutManager.CurrentLoadoutName ?? "(no loadout)"}{(loadoutManager.IsModified ? " *" : string.Empty)}";
        }

        private void LoadoutManager_LoadoutChanged(object sender, LoadoutNameEventArgs e)
        {
            UpdateLoadoutText();
        }

        private void LoadoutManager_ModifiedChanged(object sender, EventArgs e)
        {
            UpdateLoadoutText();
        }

        private void OpenSkillSelector(object parameter)
        {
            RoutedCommands.OpenSkillsSelector.ExecuteIfPossible(null);
        }

        private void AdvancedSearch(object parameter)
        {
            RoutedCommands.OpenAdvancedSearch.ExecuteIfPossible(null);
        }

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

        private CancellationTokenSource searchCancellationTokenSource;
        private Task previousSearchTask;

        private async Task SearchArmorSetsInternal()
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

            searchCancellationTokenSource = new CancellationTokenSource();
            previousSearchTask = Task.Run(() => SearchArmorSetsInternal(searchCancellationTokenSource.Token));

            IsSearching = true;

            try
            {
                await previousSearchTask;
            }
            finally
            {
                IsSearching = false;

                previousSearchTask = null;
                searchCancellationTokenSource = null;
            }
        }

        public void CreateSolverData()
        {
            SolverData = null;

            if (IsDataLoaded == false || SelectedAbilities == null)
                return;

            var desiredAbilities = SelectedAbilities
                .Where(x => x.IsChecked)
                .Select(x => x.Ability)
                .ToList();

            SolverData = new SolverData(
                InParameters.Slots.Select(x => x.Value).ToList(),
                null,
                GlobalData.Instance.Heads,
                GlobalData.Instance.Chests,
                GlobalData.Instance.Gloves,
                GlobalData.Instance.Waists,
                GlobalData.Instance.Legs,
                GlobalData.Instance.Charms,
                GlobalData.Instance.Jewels.Select(x => new SolverDataJewelModel(x)).ToList(),
                desiredAbilities
            );

            SolverData.Done();

            /*************************************************************/
            var sb = new StringBuilder();

            long hh = SolverData.AllHeads.Count(x => x.IsSelected);
            long cc = SolverData.AllChests.Count(x => x.IsSelected);
            long gg = SolverData.AllGloves.Count(x => x.IsSelected);
            long ww = SolverData.AllWaists.Count(x => x.IsSelected);
            long ll = SolverData.AllLegs.Count(x => x.IsSelected);
            long ch = SolverData.AllCharms.Count(x => x.IsSelected);

            var nfi = new System.Globalization.NumberFormatInfo();
            nfi.NumberGroupSeparator = "'";

            sb.AppendLine($"Heads count:  {hh}");
            sb.AppendLine($"Chests count: {cc}");
            sb.AppendLine($"Gloves count: {gg}");
            sb.AppendLine($"Waists count: {ww}");
            sb.AppendLine($"Legs count:   {ll}");
            sb.AppendLine($"Charms count:   {ch}");
            sb.AppendLine($"Min sLot size: {SolverData.MinJewelSize}");
            sb.AppendLine($"Max sLot size: {SolverData.MaxJewelSize}");
            sb.AppendLine($"Combination count: {(hh * cc * gg * ww * ll * ch).ToString("N0", nfi)}");

            SearchResult = sb.ToString();
            /*************************************************************/
        }

        private async Task SearchArmorSetsInternal(CancellationToken cancellationToken)
        {
            solver = new Solver(SolverData);

            solver.DebugData += Solver_DebugData;

            IList<ArmorSetSearchResult> result = await solver.SearchArmorSets(cancellationToken);

            if (result == null)
                FoundArmorSets = null;
            else
            {
                FoundArmorSets = result.Where(x => x.IsMatch).Select(x => new ArmorSetViewModel(
                    x.ArmorPieces,
                    x.Charm,
                    x.Jewels.Select(j => new ArmorSetJewelViewModel(j.Jewel, j.Count)).ToList(),
                    x.SpareSlots
                ));
            }

            solver.DebugData -= Solver_DebugData;
        }

        internal void SelectedAbilitiesChanged()
        {
            AbilitiesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Solver_DebugData(string debugData)
        {
            SearchResult = debugData;
        }

        private MaximizedSearchCriteria[] sortCriterias = new MaximizedSearchCriteria[]
        {
            MaximizedSearchCriteria.BaseDefense,
            MaximizedSearchCriteria.DragonResistance,
            MaximizedSearchCriteria.SlotSizeCube
        };

        private string searchResult;
        public string SearchResult
        {
            get { return searchResult; }
            private set { SetValue(ref searchResult, value); }
        }
    }
}
