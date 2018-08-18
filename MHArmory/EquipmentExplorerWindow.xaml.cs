using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MHArmory.ViewModels;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for EquipmentExplorerWindow.xaml
    /// </summary>
    public partial class EquipmentExplorerWindow : Window
    {
        private static readonly EquipmentExplorerWindow instance = new EquipmentExplorerWindow();
        private readonly EquipmentExplorerViewModel equipmentExplorerViewModel;

        private EquipmentExplorerWindow()
        {
            InitializeComponent();

            equipmentExplorerViewModel = new EquipmentExplorerViewModel();

            Loaded += EquipmentExplorerWindow_Loaded;
        }

        public static void Show(Window owner)
        {
            instance.Owner = owner;
            instance.ShowDialog();
        }

        private async void EquipmentExplorerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);

            equipmentExplorerViewModel.CreateItems();

            DataContext = equipmentExplorerViewModel;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            Hide();
        }
    }
}
