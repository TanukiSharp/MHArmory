using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using MHArmory.ViewModels;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly RootViewModel rootViewModel = new RootViewModel();

        private SkillSelectorWindow skillSelectorWindow;
        private AdvancedSearchWindow advancedSearchWindow;

        public MainWindow()
        {
            InitializeComponent();

            WindowManager.FitInScreen(this);

            LoadConfiguration();

            WindowManager.NotifyConfigurationLoaded();
            rootViewModel.NotifyConfigurationLoaded();

            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenSkillsSelector, OpenSkillSelector));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenAdvancedSearch, OpenAdvancedSearch));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenDecorationsOverride, OpenDecorationsOverride));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenEquipmentExplorer, OpenEquipmentExplorer));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenSearchResultProcessing, OpenSearchResultProcessing));

            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.NewLoadout, OnNewLoadout));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenLoadout, OnOpenLoadout));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.SaveLoadout, OnSaveLoadout));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.SaveLoadoutAs, OnSaveLoadoutAs));
            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.ManageLoadouts, OnManageLoadouts));

            Title = $"{App.ApplicationName} {App.DisplayVersion}";

            DataContext = rootViewModel;

            WindowManager.RestoreWindowState(this);

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Dispatcher.Yield(DispatcherPriority.Render);

            skillSelectorWindow = new SkillSelectorWindow { Owner = this };
            WindowManager.InitializeWindow(skillSelectorWindow);

            advancedSearchWindow = new AdvancedSearchWindow(rootViewModel) { Owner = this };
            WindowManager.InitializeWindow(advancedSearchWindow);

            await LoadData();

            loadoutManager = new LoadoutManager(rootViewModel);
            rootViewModel.SetLoadoutManager(loadoutManager);

            string lastOpenedLoadout = GlobalData.Instance.Configuration.LastOpenedLoadout;
            Dictionary<string, SkillLoadoutItemConfigurationV2[]> loadout = GlobalData.Instance.Configuration.SkillLoadouts;

            if (loadout != null && lastOpenedLoadout != null && loadout.TryGetValue(lastOpenedLoadout, out SkillLoadoutItemConfigurationV2[] abilities))
                loadoutManager.Open(lastOpenedLoadout, abilities);

            rootViewModel.IsDataLoading = false;
            rootViewModel.IsDataLoaded = true;

            rootViewModel.CreateSolverData();
        }

        private void LoadConfiguration()
        {
            ConfigurationV2 configuration = ConfigurationManager.Load<ConfigurationV2>();

            // Possibly process stuffs.

            GlobalData.Instance.Configuration = configuration;
        }

        public class LambdaEqualityComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> equals;
            private readonly Func<T, int> getHashCode;

            public LambdaEqualityComparer(Func<T, T, bool> equals)
                : this(equals, _ => 0)
            {
            }

            public LambdaEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
            {
                this.equals = equals;
                this.getHashCode = getHashCode;
            }

            public bool Equals(T x, T y)
            {
                return equals(x, y);
            }

            public int GetHashCode(T obj)
            {
                return getHashCode(obj);
            }
        }

        public static int ComputeDamereauLevensheinDistance(string s, string t)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                if (string.IsNullOrWhiteSpace(t))
                    return 0;

                return t.Length;
            }

            if (string.IsNullOrWhiteSpace(t))
                return s.Length;

            int n = s.Length;
            int m = t.Length;

            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++)
                ;
            for (int j = 1; j <= m; d[0, j] = j++)
                ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;

                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }

            return d[n, m];
        }

        private async Task LoadData()
        {
            //var source2 = new MhwDbDataSource.DataSource(null, App.HasWriteAccess);
            //ISkill[] skills2 = await source2.GetSkills();
            //IArmorPiece[] armors2 = await source2.GetArmorPieces();
            //ICharm[] charms2 = await source2.GetCharms();
            //IJewel[] jewels2 = await source2.GetJewels();

            //// --------------------------------------------------

            var source = new AthenaAssDataSource.DataSource();

            ISkill[] skills = await source.GetSkills();
            IArmorPiece[] armors = await source.GetArmorPieces();
            ICharm[] charms = await source.GetCharms();
            IJewel[] jewels = await source.GetJewels();



            ///***********************************************************************/

            //ISkill[] testSkills = skills.Except(skills2, new LambdaEqualityComparer<ISkill>((x, y) => x.Name == y.Name)).ToArray();
            //IArmorPiece[] testArmors = armors.Except(armors2, new LambdaEqualityComparer<IArmorPiece>((x, y) => x.Name == y.Name)).ToArray();

            //foreach (IArmorPiece a2 in armors2)
            //{
            //    if (armors.Any(a1 => a1.Name == a2.Name))
            //        continue;

            //    IArmorPiece[] bestByNameDistance = armors
            //        .Select(a1 => new { Armor = a1, Distance = ComputeDamereauLevensheinDistance(a1.Name, a2.Name) })
            //        .OrderBy(x => x.Distance)
            //        .Select(x => x.Armor)
            //        .ToArray();

            //    if (bestByNameDistance[0].Name != a2.Name)
            //    {
            //    }

            //    Console.WriteLine(bestByNameDistance[0].Name);
            //}

            //foreach (IArmorPiece a1 in armors)
            //{
            //    if (armors2.Any(a2 => a2.Name == a1.Name))
            //        continue;

            //    IArmorPiece[] bestByNameDistance = armors2
            //        .Select(a2 => new { Armor = a2, Distance = ComputeDamereauLevensheinDistance(a2.Name, a1.Name) })
            //        .OrderBy(x => x.Distance)
            //        .Select(x => x.Armor)
            //        .ToArray();

            //    if (bestByNameDistance[0].Name != a1.Name)
            //    {
            //    }

            //    Console.WriteLine(bestByNameDistance[0].Name);
            //}

            /***********************************************************************/


            if (skills == null || armors == null || charms == null || jewels == null)
            {
                CloseApplicationBecauseOfDataSource(source.Description);
                return;
            }

            IList<SkillViewModel> allSkills = skills
                .OrderBy(x => x.Name)
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

            WindowManager.ShowDialog<DecorationsOverrideWindow>();
        }

        private void OpenEquipmentExplorer(object obj)
        {
            if (WindowManager.IsInitialized<EquipmentExplorerWindow>() == false)
                WindowManager.InitializeWindow(new EquipmentExplorerWindow { Owner = this });

            WindowManager.Show<EquipmentExplorerWindow>();
        }

        private void OpenSearchResultProcessing(object parameter)
        {
            if (WindowManager.IsInitialized<SearchResultProcessingWindow>() == false)
                WindowManager.InitializeWindow(new SearchResultProcessingWindow(rootViewModel) { Owner = this });

            WindowManager.Show<SearchResultProcessingWindow>();
        }

        private LoadoutManager loadoutManager;

        private void OnNewLoadout(object parameter)
        {
            loadoutManager.Close();
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
