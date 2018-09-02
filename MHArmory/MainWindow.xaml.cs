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

        public MainWindow()
        {
            InitializeComponent();

            WindowManager.FitInScreen(this);

            LoadConfiguration();
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

            AssemblyName asmName = Assembly.GetEntryAssembly().GetName();
            Title = $"{asmName.Name} {asmName.Version.Major}.{asmName.Version.Minor}.{asmName.Version.Build}";

            DataContext = rootViewModel;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Dispatcher.Yield(DispatcherPriority.Render);

            skillSelectorWindow = new SkillSelectorWindow
            {
                Owner = this
            };

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


            if (skills == null)
            {
                CloseApplicationBecauseOfDataSource(((ISkillDataSource)source).Description);
                return;
            }
            else if (armors == null)
            {
                CloseApplicationBecauseOfDataSource(((IArmorDataSource)source).Description);
                return;
            }
            else if (charms == null)
            {
                CloseApplicationBecauseOfDataSource(((ICharmDataSource)source).Description);
                return;
            }
            else if (jewels == null)
            {
                CloseApplicationBecauseOfDataSource(((IJewelDataSource)source).Description);
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
            if (skillSelectorWindow.WindowState == WindowState.Minimized)
                skillSelectorWindow.WindowState = WindowState.Normal;

            skillSelectorWindow.Show();
        }

        private AdvancedSearchWindow advancedSearchWindow;

        private void OpenAdvancedSearch(object parameter)
        {
            if (advancedSearchWindow == null)
            {
                advancedSearchWindow = new AdvancedSearchWindow(rootViewModel)
                {
                    Owner = this
                };
            }

            advancedSearchWindow.Update();
            advancedSearchWindow.Show();
        }

        private void OpenDecorationsOverride(object obj)
        {
            var window = new DecorationsOverrideWindow(rootViewModel)
            {
                Owner = this
            };
            window.ShowDialog();
        }

        private void OpenEquipmentExplorer(object obj)
        {
            EquipmentExplorerWindow.Show(this);
        }

        private void OpenSearchResultProcessing(object parameter)
        {
            SearchResultProcessingWindow.Open(() =>
            {
                return new SearchResultProcessingWindow(rootViewModel)
                {
                    Owner = this
                };
            });
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

            if (loadoutManager.ApplicationClose() == false)
            {
                e.Cancel = true;
                return;
            }

            loadoutManager.Dispose();

            skillSelectorWindow.ApplicationClose();
        }
    }
}
