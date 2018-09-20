using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MHArmory.ViewModels;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for WeaponsWindow.xaml
    /// </summary>
    public partial class WeaponsWindow : Window
    {
        private readonly RootViewModel rootViewModel;

        public WeaponsWindow(RootViewModel rootViewModel)
        {
            InitializeComponent();

            WindowManager.FitInScreen(this);

            InputBindings.Add(new InputBinding(new AnonymousCommand(OnCancel), new KeyGesture(Key.Escape, ModifierKeys.None)));

            this.rootViewModel = rootViewModel;

            DataContext = rootViewModel.InParameters.Weapons.Where(x => x.Type == "great-sword").ToList();

            this.Loaded += WeaponsWindow_Loaded;
        }

        private async void WeaponsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (WeaponViewModel rootWeapon in rootViewModel.InParameters.Weapons)
                await Expand(rootWeapon);
        }

        private async Task Expand(WeaponViewModel vm)
        {
            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);

            if (vm.Branches != null)
            {
                vm.IsExpanded = true;
                foreach (WeaponViewModel child in vm.Branches)
                    await Expand(child);
            }
        }

        private void OnCancel()
        {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            Hide();

            rootViewModel.Events.UpdateAndSaveConfiguration();
        }
    }
}
