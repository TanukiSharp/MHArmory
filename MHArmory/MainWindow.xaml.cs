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
            skillSelectorWindow = new SkillSelectorWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var source = new MhwDbDataSource.DataSource(null);

            ISkill[] skills = await source.GetSkills();
            IArmorPiece[] armors = await source.GetArmorPieces();

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

            await Dispatcher.Yield(DispatcherPriority.Render);

            SkillViewModel[] allSkills = skills
                .OrderBy(x => x.Name)
                .Select(x => new SkillViewModel(x, rootViewModel))
                .ToArray();

            AbilityViewModel[] allAbilities = allSkills
                .SelectMany(x => x.Abilities)
                .ToArray();

            rootViewModel.SelectedAbilities = allAbilities;

            GlobalData.Instance.SetSkills(allSkills);
            GlobalData.Instance.SetAbilities(allAbilities);

            var skillsToArmorsMap = new Dictionary<int, IArmorPiece[]>();

            foreach (ISkill skill in skills)
            {
                skillsToArmorsMap.Add(skill.Id, armors
                    .Where(x => x.Abilities.Any(a => a.Skill.Id == skill.Id))
                    .ToArray()
                );
            }

            GlobalData.Instance.SetSkillsToArmorsMap(skillsToArmorsMap);
        }

        private void OpenSkillSelector(object parameter)
        {
            skillSelectorWindow.ShowDialog();
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
