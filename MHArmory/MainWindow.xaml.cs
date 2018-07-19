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

            CommandBindings.Add(RoutedCommands.CreateCommandBinding(RoutedCommands.OpenSkillsSelector, OpenSkillSelector));

            AssemblyName asmName = Assembly.GetEntryAssembly().GetName();
            Title = $"{asmName.Name} {asmName.Version.Major}.{asmName.Version.Minor}.{asmName.Version.Build}";

            DataContext = rootViewModel;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Dispatcher.Yield(DispatcherPriority.Render);

            LoadConfiguration();

            skillSelectorWindow = new SkillSelectorWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            await LoadData();

            rootViewModel.IsDataLoading = false;
            rootViewModel.IsDataLoaded = true;
        }

        private void LoadConfiguration()
        {
            Configuration configuration = Configuration.Load();

            // Possibly process stuffs.

            GlobalData.Instance.Configuration = configuration;
        }

        private async Task LoadData()
        {
            var source = new MhwDbDataSource.DataSource(null);

            ISkill[] skills = await source.GetSkills();
            IArmorPiece[] armors = await source.GetArmorPieces();
            ICharm[] charms = await source.GetCharms();
            IJewel[] jewels = await source.GetJewels();

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

            int[] configSelectedAbilities = GlobalData.Instance.Configuration.SelectedAbilities;
            if (configSelectedAbilities != null)
            {
                foreach (AbilityViewModel vm in allAbilities)
                {
                    if (configSelectedAbilities.Contains(vm.Id))
                        vm.IsChecked = true;
                }
            }

            GlobalData.Instance.SetSkills(skills);
            GlobalData.Instance.SetArmors(armors);
            GlobalData.Instance.Charms = charms.SelectMany(x => x.Levels).ToList();
            GlobalData.Instance.Jewels = jewels;

            rootViewModel.FoundArmorSets = new ArmorSetViewModel[]
            {
                new ArmorSetViewModel { ArmorPieces = armors.Skip(armors.Length - 10).Take(5).ToList(), Charm = charms[0].Levels[0] },
                new ArmorSetViewModel { ArmorPieces = armors.Skip(armors.Length - 5).ToList(), Charm = charms[1].Levels[0] }
            };
        }

        private void OpenSkillSelector(object parameter)
        {
            skillSelectorWindow.Show();
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
            skillSelectorWindow.ApplicationClose();
        }
    }
}
