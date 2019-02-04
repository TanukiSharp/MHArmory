using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MHArmory.Configurations;
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

    public sealed class LoadoutManager : IDisposable
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

                // Postponing rest of the IsModified flag to false to the next scheduler frame is
                // because of a change in the way ability/skill checkboxes are processed, for performance reasons.
                rootViewModel.Dispatcher.BeginInvoke((Action)delegate { IsModified = false; });

                CurrentLoadoutName = null;

                ConfigurationManager.Save(GlobalData.Instance.Configuration);
            }

            return result;
        }

        private static YesNoCancel SaveDialog()
        {
            MessageBoxResult dlgResult = MessageBox.Show("Do you want to save loadout modification ?", "Save loadout ?", MessageBoxButton.YesNoCancel);
            if (dlgResult == MessageBoxResult.Cancel)
                return YesNoCancel.Cancel;

            if (dlgResult == MessageBoxResult.Yes)
                return YesNoCancel.Yes;

            return YesNoCancel.No;
        }

        private void InternalReset()
        {
            if (CurrentLoadoutName == null || IsModified == false)
                return;

            MessageBoxResult dlgResult = MessageBox.Show("Do you want to reset loadout modifications ?", "Reset loadout ?", MessageBoxButton.YesNo);

            if (dlgResult == MessageBoxResult.No)
                return;

            LoadLoadoutFromConfig(GlobalData.Instance.Configuration.SkillLoadouts[CurrentLoadoutName]);

            // Postponing rest of the IsModified flag to false to the next scheduler frame is
            // because of a change in the way ability/skill checkboxes are processed, for performance reasons.
            rootViewModel.Dispatcher.BeginInvoke((Action)delegate { IsModified = false; });
        }

        private bool InternalOpen(string loadoutName, SkillLoadoutItemConfigurationV3 loadoutConfig)
        {
            if (IsModified)
            {
                if (InternalClose(false) == false)
                    return false;
            }

            if (loadoutName == null || loadoutConfig == null)
            {
                var loadoutWindow = new LoadoutWindow(false, currentLoadoutName, rootViewModel.SelectedAbilities)
                {
                    Owner = Application.Current.MainWindow
                };

                WindowManager.RestoreWindowState(loadoutWindow);
                bool? showResult = loadoutWindow.ShowDialog();
                WindowManager.StoreWindowState(loadoutWindow);

                if (showResult != true)
                    return false;

                bool hasLoadoutChanged = CurrentLoadoutName != loadoutWindow.SelectedLoadout.Name;

                if (isModified || hasLoadoutChanged)
                    LoadLoadoutFromViewModel(loadoutWindow.SelectedLoadout);

                if (hasLoadoutChanged)
                {
                    CurrentLoadoutName = loadoutWindow.SelectedLoadout.Name;
                    ConfigurationManager.Save(GlobalData.Instance.Configuration);
                }
            }
            else
            {
                LoadLoadoutFromConfig(loadoutConfig);
                CurrentLoadoutName = loadoutName;
            }

            // Postponing rest of the IsModified flag to false to the next scheduler frame is
            // because of a change in the way ability/skill checkboxes are processed, for performance reasons.
            rootViewModel.Dispatcher.BeginInvoke((Action)delegate { IsModified = false; });

            return true;
        }

        private void LoadLoadoutFromViewModel(LoadoutViewModel loadoutViewModel)
        {
            for (int i = 0; i < rootViewModel.InParameters.Slots.Length; i++)
            {
                if (i < loadoutViewModel.WeaponSlots.Length)
                    rootViewModel.InParameters.Slots[i].Value = loadoutViewModel.WeaponSlots[i];
                else
                    rootViewModel.InParameters.Slots[i].Value = 0;
            }

            foreach (AbilityViewModel ability in rootViewModel.SelectedAbilities)
                ability.IsChecked = loadoutViewModel.Abilities.Any(a => a.SkillId == ability.SkillId && a.Level == ability.Level);
        }

        private void LoadLoadoutFromConfig(SkillLoadoutItemConfigurationV3 loadoutConfig)
        {
            for (int i = 0; i < rootViewModel.InParameters.Slots.Length; i++)
            {
                if (i < loadoutConfig.WeaponSlots.Length)
                    rootViewModel.InParameters.Slots[i].Value = loadoutConfig.WeaponSlots[i];
                else
                    rootViewModel.InParameters.Slots[i].Value = 0;
            }

            foreach (AbilityViewModel ability in rootViewModel.SelectedAbilities)
            {
                ability.IsChecked = loadoutConfig.Skills.Any(x =>
                    x.SkillName == Core.Localization.GetDefault(ability.SkillName) &&
                    x.Level == ability.Level
                );
            }
        }

        private bool InternalSave()
        {
            if (IsModified == false)
                return true;

            if (CurrentLoadoutName == null)
                return InternalSaveAs();
            else
            {
                Dictionary<string, SkillLoadoutItemConfigurationV3> loadoutConfig = GlobalData.Instance.Configuration.SkillLoadouts;

                loadoutConfig[CurrentLoadoutName] = new SkillLoadoutItemConfigurationV3
                {
                    WeaponSlots = rootViewModel.InParameters.Slots.Where(x => x.Value > 0).Select(x => x.Value).ToArray(),
                    Skills = CreateSelectedSkillsArray()
                };

                IsModified = false;

                ConfigurationManager.Save(GlobalData.Instance.Configuration);

                return true;
            }
        }

        private SkillLoadoutItemConfigurationV2[] CreateSelectedSkillsArray()
        {
            return rootViewModel.SelectedAbilities
                .Where(x => x.IsChecked)
                .Select(x => new SkillLoadoutItemConfigurationV2
                {
                    SkillName = Core.Localization.GetDefault(x.Ability.Skill.Name),
                    Level = x.Level
                })
                .ToArray();
        }

        private bool InternalSaveAs()
        {
            var inputWindow = new TextInputWindow("Save loadout", "Enter loadout unique name:", currentLoadoutName, true);

            if (inputWindow.ShowDialog() != true)
                return false;

            Dictionary<string, SkillLoadoutItemConfigurationV3> loadoutConfig = GlobalData.Instance.Configuration.SkillLoadouts;

            if (loadoutConfig.ContainsKey(inputWindow.Text))
            {
                MessageBoxResult dlgResult = MessageBox.Show($"The loadout '{inputWindow.Text}' already exist. Do you want to overwrite it ?", "Overwrite loadout ?", MessageBoxButton.YesNo);
                if (dlgResult == MessageBoxResult.No)
                    return false;
            }

            loadoutConfig[inputWindow.Text] = new SkillLoadoutItemConfigurationV3
            {
                WeaponSlots = rootViewModel.InParameters.Slots.Where(x => x.Value > 0).Select(x => x.Value).ToArray(),
                Skills = CreateSelectedSkillsArray()
            };

            GlobalData.Instance.Configuration.LastOpenedLoadout = inputWindow.Text;

            CurrentLoadoutName = inputWindow.Text;
            IsModified = false;

            ConfigurationManager.Save(GlobalData.Instance.Configuration);

            return true;
        }

        private bool InternalManage()
        {
            if (IsModified)
            {
                if (InternalClose(true) == false)
                    return false;
            }

            var loadoutWindow = new LoadoutWindow(true, currentLoadoutName, rootViewModel.SelectedAbilities)
            {
                Owner = Application.Current.MainWindow
            };

            WindowManager.RestoreWindowState(loadoutWindow);
            loadoutWindow.ShowDialog();
            WindowManager.StoreWindowState(loadoutWindow);

            if (loadoutWindow.CurrentLoadoutName != currentLoadoutName)
            {
                // Currently selected skill loadout has been renamed.

                currentLoadoutName = loadoutWindow.CurrentLoadoutName;
                GlobalData.Instance.Configuration.LastOpenedLoadout = currentLoadoutName;
                LoadoutChanged?.Invoke(this, new LoadoutNameEventArgs(currentLoadoutName));
            }

            ConfigurationManager.Save(GlobalData.Instance.Configuration);

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

        public void Reset()
        {
            InternalReset();
        }

        public void Open(string loadoutName, SkillLoadoutItemConfigurationV3 loadoutConfig)
        {
            if (loadoutName == null)
                throw new ArgumentNullException(nameof(loadoutName));
            if (loadoutConfig == null)
                throw new ArgumentNullException(nameof(loadoutConfig));

            InternalOpen(loadoutName, loadoutConfig);
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

        internal void ManageLoadouts()
        {
            InternalManage();
        }
    }
}
