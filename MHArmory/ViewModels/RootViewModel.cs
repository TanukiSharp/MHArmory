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
using System.Windows.Threading;
using MHArmory.Configurations;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Search;
using MHArmory.Search.Contracts;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public sealed class RootViewModel : ViewModelBase, IDisposable
    {
        public ICommand CloseApplicationCommand { get; }

        public ICommand OpenSkillSelectorCommand { get; }
        public ICommand AdvancedSearchCommand { get; }
        public ICommand OpenDecorationsOverrideCommand { get; }
        public ICommand OpenEquipmentOverrideCommand { get; }
        public ICommand OpenSearchResultProcessingCommand { get; }

        public ICommand AboutCommand { get; }

        public event EventHandler AbilitiesChanged;

        public ExtensionSelectorViewModel Extensions { get; }

        public AutoUpdateViewModel AutoUpdateViewModel { get; } = new AutoUpdateViewModel();

        public SearchResultsViewModel SearchResultsViewModel { get; }
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

            EquipmentOverride.NotifyDataLoaded();
        }

        public InParametersViewModel InParameters { get; }

        private bool isSearching;
        public bool IsSearching
        {
            get { return isSearching; }
            internal set { SetValue(ref isSearching, value); }
        }

        private bool isAutoSearch;
        public bool IsAutoSearch
        {
            get { return isAutoSearch; }
            set { SetValue(ref isAutoSearch, value); }
        }

        public WeaponsContainerViewModel WeaponsContainer { get; }

        public EquipmentOverrideViewModel EquipmentOverride { get; }

        public IReadOnlyList<EquipmentViewModel> AllEquipments { get; internal set; }

        public LanguageViewModel[] Languages { get; private set; }

        public RootViewModel()
        {
            SearchResultsViewModel = new SearchResultsViewModel(this);

            CloseApplicationCommand = new AnonymousCommand(OnCloseApplication);

            OpenSkillSelectorCommand = new AnonymousCommand(OpenSkillSelector);
            AdvancedSearchCommand = new AnonymousCommand(AdvancedSearch);
            OpenDecorationsOverrideCommand = new AnonymousCommand(OpenDecorationsOverride);
            OpenEquipmentOverrideCommand = new AnonymousCommand(OpenEquipmentOverride);
            OpenSearchResultProcessingCommand = new AnonymousCommand(OpenSearchResultProcessing);

            AboutCommand = new AnonymousCommand(OnAbout);

            SearchResultProcessing = new SearchResultProcessingViewModel(this);
            InParameters = new InParametersViewModel(this);
            WeaponsContainer = new WeaponsContainerViewModel(this);
            EquipmentOverride = new EquipmentOverrideViewModel(this);

            SetupLocalization();

            Extensions = new ExtensionSelectorViewModel(this);
        }

        private void SetupLocalization()
        {
            Localization.Language = GlobalData.Instance.Configuration.Language ?? Localization.DefaultLanguage;

            Languages = Localization.AvailableLanguageCodes
                .Select(kv => new LanguageViewModel(kv.Key, kv.Value))
                .ToArray();

            Localization.LanguageChanged += Localization_LanguageChanged;
        }

        private void Localization_LanguageChanged(object sender, EventArgs e)
        {
            GlobalData.Instance.Configuration.Language = Localization.Language;
            ConfigurationManager.Save(GlobalData.Instance.Configuration);
        }

        public void Dispose()
        {
            Localization.LanguageChanged -= Localization_LanguageChanged;

            if (loadoutManager != null)
            {
                loadoutManager.LoadoutChanged -= LoadoutManager_LoadoutChanged;
                loadoutManager.ModifiedChanged -= LoadoutManager_ModifiedChanged;
            }
        }

        private void OnAbout()
        {
            new AboutWindow() { Owner = App.Current.MainWindow }.Show();
        }

        private void OnCloseApplication(object parameters)
        {
            App.Current.MainWindow.Close();
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

        public void SetAllEquipments(IList<EquipmentViewModel> allEquipments)
        {
            if (AllEquipments != null)
                throw new InvalidOperationException("Operation sealed");

            AllEquipments = new ReadOnlyCollection<EquipmentViewModel>(allEquipments);
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

        private void OpenEquipmentOverride()
        {
            RoutedCommands.OpenEquipmentOverride.ExecuteIfPossible(null);
        }

        private void OpenSearchResultProcessing()
        {
            RoutedCommands.OpenSearchResultProcessing.ExecuteIfPossible(null);
        }

        private ExtensionCategoryViewModelBase GetExtensionCategory(ExtensionCategory category)
        {
            ExtensionCategoryViewModelBase categoryViewModel = Extensions.Categories.FirstOrDefault(x => x.Category == category);

            if (categoryViewModel == null)
                throw new InvalidOperationException($"Extension category '{category}' unavailable");

            return categoryViewModel;
        }

        private ExtensionViewModel GetSingleSelectedExtension(ExtensionCategoryViewModelBase category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            return category.Extensions.Single(x => x.IsActive);
        }

        private ExtensionViewModel GetSingleSelectedExtension(ExtensionCategory category)
        {
            ExtensionCategoryViewModelBase categoryViewModel = GetExtensionCategory(category);
            return GetSingleSelectedExtension(categoryViewModel);
        }

        private T GetSingleSelectedExtension<T>(ExtensionCategory category)
        {
            return (T)GetSingleSelectedExtension(category).Extension;
        }

        public ISolver GetSelectedSolver()
        {
            return GetSingleSelectedExtension<ISolver>(ExtensionCategory.Solver);
        }

        public ISolverData GetSelectedSolverData()
        {
            return GetSingleSelectedExtension<ISolverData>(ExtensionCategory.SolverData);
        }

        public void CreateSolverData()
        {
            if (SearchResultsViewModel.IsDataLoaded == false || SelectedAbilities == null)
                return;

            ISolverData solverData = GetSelectedSolverData();

            var desiredAbilities = SelectedAbilities
                .Where(x => x.IsChecked)
                .Select(x => x.Ability)
                .ToList();

            solverData.Setup(
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

            /*************************************************************/
            var metrics = new SearchMetrics
            {
                Heads = solverData.AllHeads.Count(x => x.IsSelected),
                Chests = solverData.AllChests.Count(x => x.IsSelected),
                Gloves = solverData.AllGloves.Count(x => x.IsSelected),
                Waists = solverData.AllWaists.Count(x => x.IsSelected),
                Legs = solverData.AllLegs.Count(x => x.IsSelected),
                Charms = solverData.AllCharms.Count(x => x.IsSelected),
            };

            if (solverData.AllJewels.Length > 0)
            {
                metrics.MinSlotSize = solverData.AllJewels.Min(x => x.Jewel.SlotSize);
                metrics.MaxSlotSize = solverData.AllJewels.Max(x => x.Jewel.SlotSize);
            }
            else
            {
                metrics.MinSlotSize = 0;
                metrics.MaxSlotSize = 0;
            }

            metrics.UpdateCombinationCount();

            SearchMetrics = metrics;
            /*************************************************************/

            UpdateAdvancedSearch();

            SearchResultsViewModel.ResetResults();
        }

        private bool DecorationMatchInParameters(IJewel jewel)
        {
            return jewel.Rarity <= InParameters.Rarity;
        }

        private bool EquipmentMatchInParameters(IEquipment equipment)
        {
            if (equipment.Rarity > InParameters.Rarity)
                return false;

            if (InParameters.UseEquipmentOverride)
            {
                EquipmentViewModel found = AllEquipments.FirstOrDefault(x => x.Name == equipment.Name);
                if (found != null && found.IsPossessed == false)
                    return false;
            }

            return true;
        }

        private bool ArmorPieceMatchInParameters(IArmorPiece armorPiece)
        {
            return EquipmentMatchInParameters(armorPiece) && IsGenderMatching(armorPiece, InParameters.Gender);
        }

        private bool IsGenderMatching(IArmorPiece armorPiece, Gender selectedGender)
        {
            // If UI says both, then all armor pieces are a match, no matter the gender of the armor piece.
            if (selectedGender == Gender.Both)
                return true;

            // If amor piece gender is both, it is a match no matter the UI selection.
            if (armorPiece.Attributes.RequiredGender == Gender.Both)
                return true;

            // Returns only armor piece with gender matching UI selected gender.
            return armorPiece.Attributes.RequiredGender == selectedGender;
        }

        public void UpdateAdvancedSearch()
        {
            ISolverData solverData = GetSelectedSolverData();

            var armorPieceTypesViewModels = new ArmorPieceTypesViewModel[]
            {
                new ArmorPieceTypesViewModel(EquipmentType.Head, solverData.AllHeads),
                new ArmorPieceTypesViewModel(EquipmentType.Chest, solverData.AllChests),
                new ArmorPieceTypesViewModel(EquipmentType.Gloves, solverData.AllGloves),
                new ArmorPieceTypesViewModel(EquipmentType.Waist, solverData.AllWaists),
                new ArmorPieceTypesViewModel(EquipmentType.Legs, solverData.AllLegs),
                new ArmorPieceTypesViewModel(EquipmentType.Charm, solverData.AllCharms)
            };

            AdvancedSearchViewModel.Update(armorPieceTypesViewModels);
        }

        private double searchProgression;
        public double SearchProgression
        {
            get { return searchProgression; }
            internal set { SetValue(ref searchProgression, value); }
        }

        private SolverDataJewelModel CreateSolverDataJewelModel(IJewel jewel)
        {
            DecorationOverrideConfigurationV2 decorationOverrideConfig = GlobalData.Instance.Configuration.InParameters?.DecorationOverride;

            if (decorationOverrideConfig != null && decorationOverrideConfig.UseOverride)
            {
                Dictionary<string, DecorationOverrideConfigurationItem> decoOverrides = decorationOverrideConfig?.Items;

                if (decoOverrides != null)
                {
                    if (decoOverrides.TryGetValue(Localization.GetDefault(jewel.Name), out DecorationOverrideConfigurationItem found) && found.IsOverriding)
                        return new SolverDataJewelModel(jewel, found.Count);
                }
            }

            return new SolverDataJewelModel(jewel, int.MaxValue);
        }

        internal void WeaponSlotsChanged()
        {
            CreateSolverData();

            if (IsAutoSearch)
                SearchResultsViewModel.SearchArmorSets();

            if (loadoutManager != null)
                loadoutManager.IsModified = true;
        }

        private DispatcherOperation abilityChangingDispatcherOperation;

        internal void SelectedAbilitiesChanged()
        {
            if (abilityChangingDispatcherOperation == null)
            {
                void SelectedAbilitiesChangedDone(object arg)
                {
                    var self = (RootViewModel)arg;

                    try
                    {
                        self.CreateSolverData();

                        if (self.IsAutoSearch)
                            self.SearchResultsViewModel.SearchArmorSets();

                        self.AbilitiesChanged?.Invoke(self, EventArgs.Empty);
                    }
                    finally
                    {
                        self.abilityChangingDispatcherOperation = null;
                    }
                }

                abilityChangingDispatcherOperation = Dispatcher.BeginInvoke((Action<object>)SelectedAbilitiesChangedDone, this);
            }
        }

        private SearchMetrics searchMetrics;
        public SearchMetrics SearchMetrics
        {
            get { return searchMetrics; }
            internal set { SetValue(ref searchMetrics, value); }
        }
    }
}
