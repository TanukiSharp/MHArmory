using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHWSaveUtils;

namespace MHArmory.ViewModels
{
    public class SaveDataSlotViewModel : ViewModelBase
    {
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetValue(ref isSelected, value); }
        }

        public int SlotNumber { get; }
        public string Name { get; }
        public uint Rank { get; }
        public string Playtime { get; }
        public uint Zeni { get; }

        public ICommand SelectionCommand { get; }

        private readonly Action onSelection;

        public DecorationsSaveSlotInfo DecorationsSaveSlotInfo { get; }

        public SaveDataSlotViewModel(Action onSelection, DecorationsSaveSlotInfo decorationsSaveSlotInfo)
        {
            this.onSelection = onSelection;

            DecorationsSaveSlotInfo = decorationsSaveSlotInfo;

            SlotNumber = decorationsSaveSlotInfo.SlotNumber;
            Name = decorationsSaveSlotInfo.Name;
            Rank = decorationsSaveSlotInfo.Rank;
            Playtime = MiscUtils.PlaytimeToGameString(decorationsSaveSlotInfo.Playtime);
            Zeni = decorationsSaveSlotInfo.Zeni;

            SelectionCommand = new AnonymousCommand(OnSelection);
        }

        private void OnSelection()
        {
            IsSelected = true;
            onSelection();
        }
    }

    public class SaveDataAccountViewModel : ViewModelBase
    {
        public string UserId { get; }
        public IReadOnlyCollection<SaveDataSlotViewModel> SaveDataSlots { get; }

        public SaveDataAccountViewModel(Action onSelection, string userId, IEnumerable<DecorationsSaveSlotInfo> saveDataSlots)
        {
            UserId = userId;

            var list = saveDataSlots
                .Select(x => new SaveDataSlotViewModel(onSelection, x))
                .ToList();

            SaveDataSlots = new ReadOnlyCollection<SaveDataSlotViewModel>(list);
        }
    }

    public class SaveDataSlotSelectorViewModel : ViewModelBase
    {
        private readonly ObservableCollection<SaveDataAccountViewModel> accounts = new ObservableCollection<SaveDataAccountViewModel>();
        public ReadOnlyObservableCollection<SaveDataAccountViewModel> Accounts { get; }

        public event EventHandler SelectionDone;

        public DecorationsSaveSlotInfo SelectedSaveSlot
        {
            get
            {
                SaveDataSlotViewModel selectedSaveDataSlot = null;

                foreach (SaveDataAccountViewModel account in Accounts)
                {
                    foreach (SaveDataSlotViewModel saveDataSlot in account.SaveDataSlots)
                    {
                        if (saveDataSlot.IsSelected)
                        {
                            selectedSaveDataSlot = saveDataSlot;
                            break;
                        }
                    }

                    if (selectedSaveDataSlot != null)
                        break;
                }

                return selectedSaveDataSlot?.DecorationsSaveSlotInfo;
            }
        }

        public SaveDataSlotSelectorViewModel()
        {
            Accounts = new ReadOnlyObservableCollection<SaveDataAccountViewModel>(accounts);
        }

        public void AddAccount(string userId, IEnumerable<DecorationsSaveSlotInfo> saveDataSlotItems)
        {
            accounts.Add(new SaveDataAccountViewModel(OnSelection, userId, saveDataSlotItems));
        }

        private void OnSelection()
        {
            SelectionDone?.Invoke(this, EventArgs.Empty);
        }
    }
}
