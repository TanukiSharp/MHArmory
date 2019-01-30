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
using MHArmory.Configurations;
using MHArmory.ViewModels;
using MHArmory.Core.WPF;
using MHWSaveUtils;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for DecorationsOverrideWindow.xaml
    /// </summary>
    public partial class DecorationsOverrideWindow : Window
    {
        private readonly RootViewModel rootViewModel;
        private readonly DecorationsOverrideViewModel decorationsOverrideViewModel;

        public bool HasDecorationOverrideChanged { get; private set; }

        public DecorationsOverrideWindow(RootViewModel rootViewModel)
        {
            InitializeComponent();

            this.rootViewModel = rootViewModel;

            decorationsOverrideViewModel = new DecorationsOverrideViewModel(GlobalData.Instance.Jewels, ProvideSaveSlotInfo);

            InputBindings.Add(new InputBinding(new AnonymousCommand(OnCancel), new KeyGesture(Key.Escape, ModifierKeys.None)));

            DataContext = decorationsOverrideViewModel;
        }

        private DecorationsSaveSlotInfo ProvideSaveSlotInfo(IList<DecorationsSaveSlotInfo> allSlots)
        {
            var dialog = new SaveDataSlotSelectorWindow()
            {
                Owner = this
            };

            dialog.Initialize(allSlots);

            if (dialog.ShowDialog() != true)
                return null;

            return (DecorationsSaveSlotInfo)dialog.SelectedSaveSlot;
        }

        private void OnCancel(object parameter)
        {
            var cancellable = new CancellationCommandArgument();

            decorationsOverrideViewModel.CancelCommand.ExecuteIfPossible(cancellable);

            if (cancellable.IsCancelled == false)
                Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            UpdateDecorationOverrideConfiguration();

            e.Cancel = true;
            Hide();
        }

        private void UpdateDecorationOverrideConfiguration()
        {
            InParametersConfigurationV2 config = GlobalData.Instance.Configuration.InParameters;

            if (decorationsOverrideViewModel.HasChanged)
            {
                Dictionary<string, DecorationOverrideConfigurationItem> items = config.DecorationOverride.Items;

                items.Clear();

                foreach (JewelOverrideViewModel vm in decorationsOverrideViewModel.Jewels.Where(x => x.IsOverriding || x.Count > 0))
                {
                    items.Add(Core.Localization.GetDefault(vm.Name), new DecorationOverrideConfigurationItem
                    {
                        IsOverriding = vm.IsOverriding,
                        Count = vm.Count
                    });
                }

                ConfigurationManager.Save(GlobalData.Instance.Configuration);

                rootViewModel.CreateSolverData();

                HasDecorationOverrideChanged = true;
            }
        }
    }
}
