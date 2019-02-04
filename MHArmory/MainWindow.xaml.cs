using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MHArmory.Configurations;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Services;
using MHArmory.ViewModels;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly RootViewModel rootViewModel;

        private SkillSelectorWindow skillSelectorWindow;
        private AdvancedSearchWindow advancedSearchWindow;

        public MainWindow()
        {
            InitializeComponent();

            WindowManager.FitInScreen(this);

            LoadConfiguration();
            LoadAliases();

            rootViewModel = new RootViewModel();

            WindowManager.NotifyConfigurationLoaded();
            rootViewModel.NotifyConfigurationLoaded();

            ApplicationCommands.SaveAs.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));

            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenSkillsSelector, OpenSkillSelector));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenAdvancedSearch, OpenAdvancedSearch));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenDecorationsOverride, OpenDecorationsOverride));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenEquipmentOverride, OpenEquipmentOverride));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenEquipmentExplorer, OpenEquipmentExplorer));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenSearchResultProcessing, OpenSearchResultProcessing));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenWeapons, OpenWeapons));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenExtensions, OpenExtensions));

            CommandBindings.Add(RoutedCommands.CreateCommandBinding(ApplicationCommands.New, OnNewLoadout));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.ResetLoadout, OnResetLoadout));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(ApplicationCommands.Open, OnOpenLoadout));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(ApplicationCommands.Save, OnSaveLoadout));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(ApplicationCommands.SaveAs, OnSaveLoadoutAs));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.ManageLoadouts, OnManageLoadouts));

            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenIntegratedHelp, OpenIntegratedHelp));

            Title = $"{App.ApplicationName} - {App.DisplayVersion}";

            DataContext = rootViewModel;

            WindowManager.RestoreWindowState(this);

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (ConfigurationManager.IsBrandNewConfiguration)
                OpenIntegratedHelp(HelpCategory.GettingStarted);

            await Dispatcher.Yield(DispatcherPriority.SystemIdle);

            skillSelectorWindow = new SkillSelectorWindow { Owner = this };
            WindowManager.InitializeWindow(skillSelectorWindow);

            advancedSearchWindow = new AdvancedSearchWindow(rootViewModel) { Owner = this };
            WindowManager.InitializeWindow(advancedSearchWindow);

            if (await LoadData() == false)
            {
                Application.Current.Shutdown();
                return;
            }

            loadoutManager = new LoadoutManager(rootViewModel);
            rootViewModel.SetLoadoutManager(loadoutManager);

            string lastOpenedLoadout = GlobalData.Instance.Configuration.LastOpenedLoadout;
            Dictionary<string, SkillLoadoutItemConfigurationV3> loadout = GlobalData.Instance.Configuration.SkillLoadouts;

            if (loadout != null && lastOpenedLoadout != null && loadout.TryGetValue(lastOpenedLoadout, out SkillLoadoutItemConfigurationV3 loadoutConfig))
                loadoutManager.Open(lastOpenedLoadout, loadoutConfig);

            rootViewModel.SearchResultsViewModel.IsDataLoading = false;
            rootViewModel.SearchResultsViewModel.IsDataLoaded = true;

            rootViewModel.NotifyDataLoaded();

            rootViewModel.CreateSolverData();
        }

        private void LoadConfiguration()
        {
            ConfigurationV3 configuration = ConfigurationManager.Load<ConfigurationV3>();

            // Possibly process stuffs.

            GlobalData.Instance.Configuration = configuration;
        }

        private void LoadAliases()
        {
            GlobalData.Instance.SetAliases(Aliases.Load());
        }

        private async Task<bool> LoadData()
        {
            IDataSource source = null;

            try
            {
                source = new ArmoryDataSource.DataSource(null);
            }
            catch (InvalidDataSourceException)
            {
                return false;
            }

            ISkill[] skills = await source.GetSkills();
            IArmorPiece[] armors = await source.GetArmorPieces();
            ICharm[] charms = await source.GetCharms();
            IJewel[] jewels = await source.GetJewels();

            if (skills == null || armors == null || charms == null || jewels == null)
            {
                CloseApplicationBecauseOfDataSource(source.Description);
                return false;
            }

            IList<SkillViewModel> allSkills = skills
                .OrderBy(x => x.Id)
                .Select(x => new SkillViewModel(x, jewels.Where(j => j.Abilities.Any(a => a.Skill.Id == x.Id)).ToList(), rootViewModel, skillSelectorWindow.SkillSelector))
                .ToList();

            skillSelectorWindow.SkillSelector.Skills = allSkills;

            IList<AbilityViewModel> allAbilities = allSkills
                .SelectMany(x => x.Abilities)
                .ToList();

            rootViewModel.SelectedAbilities = allAbilities;

            GlobalData.Instance.SetSkills(skills);
            GlobalData.Instance.SetArmors(armors);
            GlobalData.Instance.Charms = charms.SelectMany(x => x.Levels).ToList();
            GlobalData.Instance.Jewels = jewels;

            rootViewModel.SetAllEquipments(
                armors.Select(x => new ArmorPieceViewModel(rootViewModel, x))
                .Concat(GlobalData.Instance.Charms.Select(x => new EquipmentViewModel(rootViewModel, x)))
                .ToList()
            );

            return true;
        }

        private void OpenIntegratedHelp(object parameter)
        {
            if (WindowManager.IsInitialized<HelpWindow>() == false)
                WindowManager.InitializeWindow(new HelpWindow() { Owner = this });

            WindowManager.Show<HelpWindow>(parameter);
        }

        private void OpenSkillSelector(object parameter)
        {
            WindowManager.Show<SkillSelectorWindow>();
        }

        private void OpenAdvancedSearch(object parameter)
        {
            advancedSearchWindow.Update();
            WindowManager.Show<AdvancedSearchWindow>();
        }

        private void OpenDecorationsOverride(object obj)
        {
            if (WindowManager.IsInitialized<DecorationsOverrideWindow>() == false)
                WindowManager.InitializeWindow(new DecorationsOverrideWindow(rootViewModel) { Owner = this });

            WindowManager.Show<DecorationsOverrideWindow>();
        }

        private void OpenEquipmentOverride(object obj)
        {
            if (WindowManager.IsInitialized<EquipmentOverrideWindow>() == false)
                WindowManager.InitializeWindow(new EquipmentOverrideWindow(rootViewModel) { Owner = this });

            WindowManager.Show<EquipmentOverrideWindow>();
        }

        private void OpenEquipmentExplorer(object obj)
        {
            if (WindowManager.IsInitialized<EquipmentExplorerWindow>() == false)
                WindowManager.InitializeWindow(new EquipmentExplorerWindow(rootViewModel) { Owner = this });

            WindowManager.Show<EquipmentExplorerWindow>();
        }

        private void OpenSearchResultProcessing(object parameter)
        {
            if (WindowManager.IsInitialized<SearchResultProcessingWindow>() == false)
                WindowManager.InitializeWindow(new SearchResultProcessingWindow(rootViewModel) { Owner = this });

            WindowManager.Show<SearchResultProcessingWindow>();
        }

        private void OpenWeapons(object obj)
        {
            if (WindowManager.IsInitialized<WeaponsWindow>() == false)
                WindowManager.InitializeWindow(new WeaponsWindow(rootViewModel) { Owner = this });

            WindowManager.Show<WeaponsWindow>();
        }

        private void OpenExtensions(object obj)
        {
            if (WindowManager.IsInitialized<ExtensionsWindow>() == false)
                WindowManager.InitializeWindow(new ExtensionsWindow(rootViewModel) { Owner = this });

            WindowManager.Show<ExtensionsWindow>();
        }

        private LoadoutManager loadoutManager;

        private void OnNewLoadout(object parameter)
        {
            loadoutManager.Close();
        }

        private void OnResetLoadout(object parameter)
        {
            loadoutManager.Reset();
        }

        private void OnOpenLoadout(object parameter)
        {
            loadoutManager.Open();
        }

        private void OnSaveLoadout(object parameter)
        {
            loadoutManager.Save();
        }

        private void OnSaveLoadoutAs(object parameter)
        {
            loadoutManager.SaveAs();
        }

        private void OnManageLoadouts(object parameter)
        {
            loadoutManager.ManageLoadouts();
        }

        private void CloseApplicationBecauseOfDataSource(string description)
        {
            string message = $"Could not load required data from '{description}'\nContact the data source owner for more information.";
            MessageBox.Show(this, message, "Data source error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (loadoutManager != null)
            {
                if (loadoutManager.ApplicationClose() == false)
                {
                    e.Cancel = true;
                    return;
                }
                loadoutManager.Dispose();
            }

            WindowManager.StoreWindowState(this);

            WindowManager.ApplicationClose();

            WindowManager.SaveWindowsConfiguration();

            Application.Current.Shutdown();
        }
    }
}
