using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            Title = $"{asmName.Name} {asmName.Version}";

            DataContext = rootViewModel;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var source = new MhwDbDataSource.DataSource(null);
            Console.WriteLine(await source.GetSkills());
        }
    }
}
