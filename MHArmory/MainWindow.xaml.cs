using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        public MainWindow()
        {
            InitializeComponent();

            AssemblyName asmName = Assembly.GetEntryAssembly().GetName();
            Title = $"{asmName.Name} {asmName.Version.Major}.{asmName.Version.Minor}.{asmName.Version.Build}";

            DataContext = rootViewModel;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var source = new MhwDbDataSource.DataSource(null);
            ISkill[] skills = await source.GetSkills();

            SkillViewModel[] allSkills = skills
                .OrderBy(x => x.Name)
                .Select(x => new SkillViewModel(x))
                .ToArray();

            GlobalData.Instance.SetSkills(allSkills);
            GlobalData.Instance.SetAbilities(allSkills.SelectMany(x => x.Abilities).ToArray());
        }
    }
}
