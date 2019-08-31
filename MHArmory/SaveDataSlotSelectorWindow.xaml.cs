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
        private SaveDataSlotSelectorViewModel<SaveSlotInfoBase> dataSlotSelectorViewModel;

        public SaveSlotInfoBase SelectedSaveSlot { get; private set; }

        public SaveDataSlotSelectorWindow()
        {
            InitializeComponent();
        }

        public void Initialize<T>(IList<T> saveSlots) where T : SaveSlotInfoBase
        {
            dataSlotSelectorViewModel = new SaveDataSlotSelectorViewModel<SaveSlotInfoBase>();

            foreach (IGrouping<string, T> group in saveSlots.GroupBy(x => x.SaveDataInfo.UserId))
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
