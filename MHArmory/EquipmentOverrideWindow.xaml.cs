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
using MHArmory.Core.WPF;
using MHWSaveUtils;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for EquipmentOverrideWindow.xaml
    /// </summary>
    public partial class EquipmentOverrideWindow : Window
    {
        private readonly RootViewModel rootViewModel;

        public EquipmentOverrideWindow(RootViewModel rootViewModel)
        {
            InitializeComponent();

            this.rootViewModel = rootViewModel;

            rootViewModel.EquipmentOverride.SetSaveSelector(ProvideSaveSlotInfo);

            InputBindings.Add(new InputBinding(new AnonymousCommand(OnCancel), new KeyGesture(Key.Escape, ModifierKeys.None)));

            this.Loaded += EquipmentOverrideWindow_Loaded;
        }

        private async void EquipmentOverrideWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.SystemIdle);

            DataContext = rootViewModel.EquipmentOverride;

            lblLoading.Visibility = Visibility.Collapsed;
            cntRoot.Visibility = Visibility.Visible;
        }

        private EquipmentsSaveSlotInfo ProvideSaveSlotInfo(IList<EquipmentsSaveSlotInfo> allSlots)
        {
            var dialog = new SaveDataSlotSelectorWindow()
            {
                Owner = this
            };

            dialog.Initialize(allSlots);

            if (dialog.ShowDialog() != true)
                return null;

            return (EquipmentsSaveSlotInfo)dialog.SelectedSaveSlot;
        }

        private void OnCancel(object parameter)
        {
            var cancellable = new CancellationCommandArgument();

            rootViewModel.EquipmentOverride.CancelCommand.ExecuteIfPossible(cancellable);

            if (cancellable.IsCancelled == false)
                Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            rootViewModel.EquipmentOverride.SaveConfiguration();

            rootViewModel.CreateSolverData();

            e.Cancel = true;
            Hide();
        }
    }
}
