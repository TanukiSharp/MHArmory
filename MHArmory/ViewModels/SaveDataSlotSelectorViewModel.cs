using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core.WPF;
using MHWSaveUtils;

namespace MHArmory.ViewModels
{
    public class SaveDataSlotViewModel<T> : ViewModelBase where T : BaseSaveSlotInfo
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

        public T SaveSlotInfo { get; }

        public SaveDataSlotViewModel(Action onSelection, T saveSlotInfo)
        {
            this.onSelection = onSelection;

            SaveSlotInfo = saveSlotInfo;

            SlotNumber = saveSlotInfo.SlotNumber;
            Name = saveSlotInfo.Name;
            Rank = saveSlotInfo.Rank;
            Playtime = MiscUtils.PlaytimeToGameString(saveSlotInfo.Playtime);
            Zeni = saveSlotInfo.Zeni;

            SelectionCommand = new AnonymousCommand(OnSelection);
        }

        private void OnSelection()
        {
            IsSelected = true;
            onSelection();
        }
    }

    public class SaveDataAccountViewModel<T> : ViewModelBase where T : BaseSaveSlotInfo
    {
        public string UserId { get; }
        public IReadOnlyCollection<SaveDataSlotViewModel<T>> SaveDataSlots { get; }

        public SaveDataAccountViewModel(Action onSelection, string userId, IEnumerable<T> saveDataSlots)
        {
            UserId = userId;

            var list = saveDataSlots
                .Select(x => new SaveDataSlotViewModel<T>(onSelection, x))
                .ToList();

            SaveDataSlots = new ReadOnlyCollection<SaveDataSlotViewModel<T>>(list);
        }
    }

    public class SaveDataSlotSelectorViewModel<T> : ViewModelBase where T : BaseSaveSlotInfo
    {
        private readonly ObservableCollection<SaveDataAccountViewModel<T>> accounts = new ObservableCollection<SaveDataAccountViewModel<T>>();
        public ReadOnlyObservableCollection<SaveDataAccountViewModel<T>> Accounts { get; }

        public event EventHandler SelectionDone;

        public T SelectedSaveSlot
        {
            get
            {
                SaveDataSlotViewModel<T> selectedSaveDataSlot = null;

                foreach (SaveDataAccountViewModel<T> account in Accounts)
                {
                    foreach (SaveDataSlotViewModel<T> saveDataSlot in account.SaveDataSlots)
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

                return selectedSaveDataSlot.SaveSlotInfo;
            }
        }

        public SaveDataSlotSelectorViewModel()
        {
            Accounts = new ReadOnlyObservableCollection<SaveDataAccountViewModel<T>>(accounts);
        }

        public void AddAccount(string userId, IEnumerable<T> saveDataSlotItems)
        {
            accounts.Add(new SaveDataAccountViewModel<T>(OnSelection, userId, saveDataSlotItems));
        }

        private void OnSelection()
        {
            SelectionDone?.Invoke(this, EventArgs.Empty);
        }
    }
}
