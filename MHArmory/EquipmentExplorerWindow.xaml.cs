using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

            InputBindings.Add(new InputBinding(new AnonymousCommand(OnCancel), new KeyGesture(Key.Escape, ModifierKeys.None)));

            Loaded += EquipmentExplorerWindow_Loaded;
        }

        private void OnCancel(object parameter)
        {
            var cancellable = new CancellationCommandArgument();

            equipmentExplorerViewModel.CancelCommand.ExecuteIfPossible(cancellable);

            if (cancellable.IsCancelled == false)
                Close();
        }

        public static void Show(Window owner)
        {
            if (instance.WindowState == WindowState.Minimized)
                instance.WindowState = WindowState.Normal;

            instance.Owner = owner;

            instance.Show();
            instance.Activate();
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
