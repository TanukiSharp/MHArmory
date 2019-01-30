using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MHArmory.ViewModels;
using MHArmory.Core.WPF;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for EquipmentExplorerWindow.xaml
    /// </summary>
    public partial class EquipmentExplorerWindow : Window
    {
        private readonly EquipmentExplorerViewModel equipmentExplorerViewModel;

        public EquipmentExplorerWindow(RootViewModel rootViewModel)
        {
            InitializeComponent();

            equipmentExplorerViewModel = new EquipmentExplorerViewModel(rootViewModel);

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

        private async void EquipmentExplorerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.SystemIdle);

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
