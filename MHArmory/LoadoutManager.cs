using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MHArmory.ViewModels;

namespace MHArmory
{
    public enum YesNoCancel
    {
        Yes,
        No,
        Cancel
    }

    public class LoadoutNameEventArgs : EventArgs
    {
        public string Name { get; }

        public LoadoutNameEventArgs(string name)
        {
            Name = name;
        }
    }

    public class LoadoutManager : IDisposable
    {
        public event EventHandler ModifiedChanged;

        private bool isModified;
        public bool IsModified
        {
            get { return isModified; }
            set
            {
                if (isModified != value)
                {
                    isModified = value;
                    ModifiedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler<LoadoutNameEventArgs> LoadoutChanged;

        private string currentLoadoutName = null;
        public string CurrentLoadoutName
        {
            get { return currentLoadoutName; }
            set
            {
                if (currentLoadoutName != value)
                {
                    currentLoadoutName = value;
                    GlobalData.Instance.Configuration.LastOpenedLoadout = currentLoadoutName;
                    LoadoutChanged?.Invoke(this, new LoadoutNameEventArgs(currentLoadoutName));
                }
            }
        }

        private readonly RootViewModel rootViewModel;

        public LoadoutManager(RootViewModel rootViewModel)
        {
            this.rootViewModel = rootViewModel;
            this.rootViewModel.AbilitiesChanged += RootViewModel_AbilitiesChanged;
        }

        public void Dispose()
        {
            rootViewModel.AbilitiesChanged -= RootViewModel_AbilitiesChanged;
        }

        private void RootViewModel_AbilitiesChanged(object sender, EventArgs e)
        {
            IsModified = true;
        }

        private bool InternalClose(bool updateState)
        {
            bool result = true;

            if (IsModified)
            {
                YesNoCancel state = SaveDialog();

                if (state == YesNoCancel.Cancel)
                    return false;

                if (state == YesNoCancel.Yes)
                {
                    if (CurrentLoadoutName != null)
                        result = InternalSave();
                    else
                        result = InternalSaveAs();
                }
            }

            if (result && updateState)
            {
                foreach (AbilityViewModel ability in rootViewModel.SelectedAbilities.Where(x => x.IsChecked))
                    ability.IsChecked = false;

                IsModified = false;

                CurrentLoadoutName = null;

                GlobalData.Instance.Configuration.Save();
            }

            return result;
        }

        private YesNoCancel SaveDialog()
        {
            MessageBoxResult dlgResult = MessageBox.Show("Do you want to save loadout modification ?", "Save loadout ?", MessageBoxButton.YesNoCancel);
            if (dlgResult == MessageBoxResult.Cancel)
                return YesNoCancel.Cancel;

            if (dlgResult == MessageBoxResult.Yes)
                return YesNoCancel.Yes;

            return YesNoCancel.No;
        }

        private bool InternalOpen(string loadoutName, int[] abilities)
        {
            if (IsModified)
            {
                if (InternalClose(true) == false)
                    return false;
            }

            if (loadoutName == null || abilities == null)
            {
                var loadoutWindow = new LoadoutWindow(rootViewModel.SelectedAbilities);
                if (loadoutWindow.ShowDialog() != true)
                    return false;

                foreach (AbilityViewModel ability in rootViewModel.SelectedAbilities)
                    ability.IsChecked = loadoutWindow.SelectedLoadout.Abilities.Any(a => a.Id == ability.Id);

                CurrentLoadoutName = loadoutWindow.SelectedLoadout.Name;
            }
            else
            {
                foreach (AbilityViewModel ability in rootViewModel.SelectedAbilities)
                    ability.IsChecked = abilities.Any(aId => aId == ability.Id);

                CurrentLoadoutName = loadoutName;
            }

            IsModified = false;

            GlobalData.Instance.Configuration.Save();

            return true;
        }

        private bool InternalSave()
        {
            if (IsModified == false)
                return true;

            if (CurrentLoadoutName == null)
                return InternalSaveAs();
            else
            {
                Dictionary<string, int[]> loadoutConfig = GlobalData.Instance.Configuration?.Loadout;

                if (loadoutConfig == null)
                {
                    loadoutConfig = new Dictionary<string, int[]>();
                    GlobalData.Instance.Configuration.Loadout = loadoutConfig;
                }

                loadoutConfig[CurrentLoadoutName] = rootViewModel.SelectedAbilities.Where(a => a.IsChecked).Select(a => a.Id).ToArray();

                IsModified = false;

                GlobalData.Instance.Configuration.Save();

                return true;
            }
        }

        private bool InternalSaveAs()
        {
            var inputWindow = new TextInputWindow("Save loadout", "Enter loadout unique name:", null, true);

            if (inputWindow.ShowDialog() != true)
                return false;

            Dictionary<string, int[]> loadoutConfig = GlobalData.Instance.Configuration?.Loadout;

            if (loadoutConfig != null)
            {
                if (loadoutConfig.ContainsKey(inputWindow.Text))
                {
                    MessageBoxResult dlgResult = MessageBox.Show($"The loadout '{inputWindow.Text}' already exist. Do you want to overwrite it ?", "Overwrite loadout ?", MessageBoxButton.YesNo);
                    if (dlgResult == MessageBoxResult.No)
                        return false;
                }
            }
            else
            {
                loadoutConfig = new Dictionary<string, int[]>();
                GlobalData.Instance.Configuration.Loadout = loadoutConfig;
            }

            loadoutConfig[inputWindow.Text] = rootViewModel.SelectedAbilities.Where(a => a.IsChecked).Select(a => a.Id).ToArray();
            GlobalData.Instance.Configuration.LastOpenedLoadout = inputWindow.Text;

            CurrentLoadoutName = inputWindow.Text;
            IsModified = false;

            GlobalData.Instance.Configuration.Save();

            return true;
        }

        public bool Close()
        {
            return InternalClose(true);
        }

        public bool ApplicationClose()
        {
            return InternalClose(false);
        }

        public void Open(string loadoutName, int[] abilities)
        {
            if (loadoutName == null)
                throw new ArgumentNullException(nameof(loadoutName));
            if (abilities == null)
                throw new ArgumentNullException(nameof(abilities));

            InternalOpen(loadoutName, abilities);
        }

        public void Open()
        {
            InternalOpen(null, null);
        }

        public void Save()
        {
            InternalSave();
        }

        public void SaveAs()
        {
            InternalSaveAs();
        }
    }
}
