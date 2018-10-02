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
using MHArmory.Configurations;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Search;

namespace MHArmory.ViewModels
{
    public class RootViewModel : ViewModelBase, IDisposable
    {
        public ICommand CloseApplicationCommand { get; }

        public ICommand OpenSkillSelectorCommand { get; }
        public ICommand SearchArmorSetsCommand { get; }
        public ICommand CancelArmorSetsSearchCommand { get; }
        public ICommand AdvancedSearchCommand { get; }
        public ICommand OpenDecorationsOverrideCommand { get; }
        public ICommand OpenSearchResultProcessingCommand { get; }

        public ICommand AboutCommand { get; }

        public event EventHandler AbilitiesChanged;

        public ISolverData SolverData { get; private set; }

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

        public AdvancedSearchViewModel AdvancedSearchViewModel { get; } = new AdvancedSearchViewModel();

        private IEnumerable<AbilityViewModel> selectedAbilities;
        public IEnumerable<AbilityViewModel> SelectedAbilities
        {
            get { return selectedAbilities; }
            set { SetValue(ref selectedAbilities, value); }
        }

        public SearchResultProcessingViewModel SearchResultProcessing { get; }

        internal void NotifyConfigurationLoaded()
        {
            SearchResultProcessing.NotifyConfigurationLoaded();
            InParameters.NotifyConfigurationLoaded();
        }

        internal void NotifyDataLoaded()
        {
            WeaponsContainer.LoadWeaponsAsync().Forget(ex => throw new Exception("rethrow", ex));

            EventContainer.NotifyDataLoaded();
        }

        private IEnumerable<ArmorSetViewModel> rawFoundArmorSets;

        private IEnumerable<ArmorSetViewModel> foundArmorSets;
        public IEnumerable<ArmorSetViewModel> FoundArmorSets
        {
            get { return foundArmorSets; }
            private set { SetValue(ref foundArmorSets, value); }
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

        public EventContainerViewModel EventContainer { get; }
        public WeaponsContainerViewModel WeaponsContainer { get; }

        public RootViewModel()
        {
            CloseApplicationCommand = new AnonymousCommand(OnCloseApplication);

            OpenSkillSelectorCommand = new AnonymousCommand(OpenSkillSelector);
            SearchArmorSetsCommand = new AnonymousCommand(SearchArmorSets);
            CancelArmorSetsSearchCommand = new AnonymousCommand(CancelArmorSetsSearchForCommand);
            AdvancedSearchCommand = new AnonymousCommand(AdvancedSearch);
            OpenDecorationsOverrideCommand = new AnonymousCommand(OpenDecorationsOverride);
            OpenSearchResultProcessingCommand = new AnonymousCommand(OpenSearchResultProcessing);

            AboutCommand = new AnonymousCommand(OnAbout);

            SearchResultProcessing = new SearchResultProcessingViewModel(this);
            InParameters = new InParametersViewModel(this);
            EventContainer = new EventContainerViewModel(this);
            WeaponsContainer = new WeaponsContainerViewModel(this);
        }

        public void Dispose()
        {
            if (loadoutManager != null)
            {
                loadoutManager.LoadoutChanged -= LoadoutManager_LoadoutChanged;
                loadoutManager.ModifiedChanged -= LoadoutManager_ModifiedChanged;
            }
        }

        private void OnAbout()
        {
            new AboutWindow() { Owner = App.Current.MainWindow }.ShowDialog();
        }

        private void OnCloseApplication(object parameters)
        {
            App.Current.MainWindow.Close();
        }

        public void ApplySorting(bool force, int limit = 200)
        {
            if (rawFoundArmorSets == null)
                return;

            IEnumerable<ArmorSetViewModel> result = rawFoundArmorSets;

            if (SearchResultProcessing.ApplySort(ref result, force, limit))
                FoundArmorSets = result;
        }

        public void ApplySorting(SearchResultProcessingContainerViewModel container, bool force, int limit = 200)
        {
            if (rawFoundArmorSets == null)
                return;

            IEnumerable<ArmorSetViewModel> result = rawFoundArmorSets;

            if (SearchResultProcessing.ApplySort(ref result, container, force, limit))
                FoundArmorSets = result;
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

        private void OpenDecorationsOverride()
        {
            RoutedCommands.OpenDecorationsOverride.ExecuteIfPossible(null);
        }

        private void OpenSearchResultProcessing()
        {
            RoutedCommands.OpenSearchResultProcessing.ExecuteIfPossible(null);
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

        private async void CancelArmorSetsSearchForCommand()
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

        private CancellationTokenSource searchCancellationTokenSource;
        private Task previousSearchTask;

        private async Task SearchArmorSetsInternal()
        {
            await CancelArmorSetsSearch();

            searchCancellationTokenSource = new CancellationTokenSource();
            previousSearchTask = Task.Run(() => SearchArmorSetsInternal(searchCancellationTokenSource.Token));

            SearchProgression = 0.0;
            IsSearching = true;

            try
            {
                await previousSearchTask;
            }
            finally
            {
                IsSearching = false;

                previousSearchTask = null;

                searchCancellationTokenSource.Cancel();
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
                GlobalData.Instance.Heads.Where(x => ArmorPieceMatchInParameters(x)),
                GlobalData.Instance.Chests.Where(x => ArmorPieceMatchInParameters(x)),
                GlobalData.Instance.Gloves.Where(x => ArmorPieceMatchInParameters(x)),
                GlobalData.Instance.Waists.Where(x => ArmorPieceMatchInParameters(x)),
                GlobalData.Instance.Legs.Where(x => ArmorPieceMatchInParameters(x)),
                GlobalData.Instance.Charms.Where(x => EquipmentMatchInParameters(x)),
                GlobalData.Instance.Jewels.Where(x => DecorationMatchInParameters(x)).Select(CreateSolverDataJewelModel),
                desiredAbilities
            );

            SolverData.Done();

            /*************************************************************/
            var metrics = new SearchMetrics
            {
                Heads = SolverData.AllHeads.Count(x => x.IsSelected),
                Chests = SolverData.AllChests.Count(x => x.IsSelected),
                Gloves = SolverData.AllGloves.Count(x => x.IsSelected),
                Waists = SolverData.AllWaists.Count(x => x.IsSelected),
                Legs = SolverData.AllLegs.Count(x => x.IsSelected),
                Charms = SolverData.AllCharms.Count(x => x.IsSelected),
                MinSlotSize = SolverData.MinJewelSize,
                MaxSlotSize = SolverData.MaxJewelSize,
            };

            metrics.UpdateCombinationCount();

            SearchMetrics = metrics;
            /*************************************************************/

            UpdateAdvancedSearch();

            rawFoundArmorSets = null;
        }

        private bool DecorationMatchInParameters(IJewel jewel)
        {
            return jewel.Rarity <= InParameters.Rarity;
        }

        private bool EquipmentMatchInParameters(IEquipment equipement)
        {
            if (CheckEvent(equipement) == false)
                return false;

            return equipement.Rarity <= InParameters.Rarity;
        }

        private bool ArmorPieceMatchInParameters(IArmorPiece armorPiece)
        {
            if (CheckEvent(armorPiece) == false)
                return false;

            return EquipmentMatchInParameters(armorPiece) && IsGenderMatching(armorPiece, InParameters.Gender);
        }

        private bool CheckEvent(IEquipment equipment)
        {
            if (equipment.Event != null && EventContainer.Events != null)
            {
                EventViewModel vm = EventContainer.Events.FirstOrDefault(x => x.Name == equipment.Event.Name);
                if (vm != null && vm.IsEnabled == false)
                    return false;
            }

            return true;
        }

        private bool IsGenderMatching(IArmorPiece armorPiece, Gender gender)
        {
            if (armorPiece.Attributes.RequiredGender == Gender.Both)
                return true;

            return armorPiece.Attributes.RequiredGender == gender;
        }

        public void UpdateAdvancedSearch()
        {
            ISolverData solverData = SolverData;

            var armorPieceTypesViewModels = new ArmorPieceTypesViewModel[]
            {
                new ArmorPieceTypesViewModel(solverData.AllHeads),
                new ArmorPieceTypesViewModel(solverData.AllChests),
                new ArmorPieceTypesViewModel(solverData.AllGloves),
                new ArmorPieceTypesViewModel(solverData.AllWaists),
                new ArmorPieceTypesViewModel(solverData.AllLegs),
                new ArmorPieceTypesViewModel(solverData.AllCharms)
            };

            AdvancedSearchViewModel.Update(armorPieceTypesViewModels);
        }

        private double searchProgression;
        public double SearchProgression
        {
            get { return searchProgression; }
            private set { SetValue(ref searchProgression, value); }
        }

        private async Task SearchArmorSetsInternal(CancellationToken cancellationToken)
        {
            solver = new Solver(SolverData);

            solver.SearchMetricsChanged += SolverSearchMetricsChanged;
            solver.SearchProgress += SolverSearchProgress;

            try
            {
                IList<ArmorSetSearchResult> result = await solver.SearchArmorSets(cancellationToken);

                if (result != null)
                {
                    rawFoundArmorSets = result.Where(x => x.IsMatch).Select(x => new ArmorSetViewModel(
                        SolverData,
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
                solver.SearchMetricsChanged -= SolverSearchMetricsChanged;
                solver.SearchProgress -= SolverSearchProgress;
            }
        }

        private void SolverSearchProgress(double progressRatio)
        {
            SearchProgression = progressRatio;
        }

        private SolverDataJewelModel CreateSolverDataJewelModel(IJewel jewel)
        {
            DecorationOverrideConfigurationV2 decorationOverrideConfig = GlobalData.Instance.Configuration.InParameters?.DecorationOverride;

            if (decorationOverrideConfig != null && decorationOverrideConfig.UseOverride)
            {
                Dictionary<string, DecorationOverrideConfigurationItem> decoOverrides = decorationOverrideConfig?.Items;

                if (decoOverrides != null)
                {
                    if (decoOverrides.TryGetValue(jewel.Name, out DecorationOverrideConfigurationItem found) && found.IsOverriding)
                        return new SolverDataJewelModel(jewel, found.Count);
                }
            }

            return new SolverDataJewelModel(jewel, int.MaxValue);
        }

        internal void SelectedAbilitiesChanged()
        {
            AbilitiesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SolverSearchMetricsChanged(SearchMetrics metricsData)
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                SearchMetrics = null;
                SearchMetrics = metricsData;
            });
        }

        private SearchMetrics searchMetrics;
        public SearchMetrics SearchMetrics
        {
            get { return searchMetrics; }
            private set
            {
                searchMetrics = value;
                NotifyPropertyChanged();
            }
        }
    }
}
