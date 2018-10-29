using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MHArmory.ViewModels;
using MHWSaveUtils;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for SaveDataSlotSelectorWindow.xaml
    /// </summary>
    public partial class SaveDataSlotSelectorWindow : Window
    {
        private readonly SaveDataSlotSelectorViewModel dataSlotSelectorViewModel;

        public DecorationsSaveSlotInfo SelectedSaveSlot { get; private set; }

        public SaveDataSlotSelectorWindow(IList<DecorationsSaveSlotInfo> saveSlots)
        {
            InitializeComponent();

            WindowManager.FitInScreen(this);

            dataSlotSelectorViewModel = new SaveDataSlotSelectorViewModel();

            foreach (IGrouping<string, DecorationsSaveSlotInfo> group in saveSlots.GroupBy(x => x.SaveDataInfo.UserId))
                dataSlotSelectorViewModel.AddAccount(group.Key, group);

            dataSlotSelectorViewModel.SelectionDone += DataSlotSelectorViewModel_SelectionDone;

            DataContext = dataSlotSelectorViewModel;
        }

        private void DataSlotSelectorViewModel_SelectionDone(object sender, EventArgs e)
        {
            SelectedSaveSlot = dataSlotSelectorViewModel.SelectedSaveSlot;
            DialogResult = SelectedSaveSlot != null;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            dataSlotSelectorViewModel.SelectionDone -= DataSlotSelectorViewModel_SelectionDone;
        }
    }
}
