using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MHArmory.Configurations;
using MHArmory.Core.WPF;
using MHArmory.ViewModels;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for LoadoutWindow.xaml
    /// </summary>
    public partial class LoadoutWindow : Window
    {
        private readonly LoadoutSelectorViewModel loadoutSelectorViewModel;

        public LoadoutViewModel SelectedLoadout
        {
            get
            {
                return loadoutSelectorViewModel.SelectedLoadout;
            }
        }

        public string CurrentLoadoutName { get; private set; }
        private readonly LoadoutViewModel currentLoadout;

        public bool IsCurrentLoadoutRenamed { get; private set; }

        public LoadoutWindow(bool isManageMode, string selectedLoadoutName, IEnumerable<AbilityViewModel> abilities)
        {
            InitializeComponent();

            loadoutSelectorViewModel = new LoadoutSelectorViewModel(isManageMode, OnEnd, abilities);

            if (selectedLoadoutName == null)
                loadoutSelectorViewModel.SelectedLoadout = null;
            else
            {
                currentLoadout = loadoutSelectorViewModel.Loadouts.FirstOrDefault(x => x.Name == selectedLoadoutName);
                if (currentLoadout != null)
                {
                    CurrentLoadoutName = currentLoadout.Name;
                    loadoutSelectorViewModel.SelectedLoadout = currentLoadout;
                }
                else
                {
                    CurrentLoadoutName = null;
                    loadoutSelectorViewModel.SelectedLoadout = null;
                }
            }

            InputBindings.Add(new KeyBinding(loadoutSelectorViewModel.AcceptCommand, new KeyGesture(Key.Enter, ModifierKeys.None)));
            InputBindings.Add(new KeyBinding(new AnonymousCommand(OnCancel), new KeyGesture(Key.Escape, ModifierKeys.None)));

            DataContext = loadoutSelectorViewModel;
        }

        private void OnCancel()
        {
            var cancellable = new CancellationCommandArgument();

            loadoutSelectorViewModel.CancelCommand.ExecuteIfPossible(cancellable);

            if (cancellable.IsCancelled == false)
                Close();
        }

        private void OnEnd(bool? value)
        {
            DialogResult = value;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (loadoutSelectorViewModel.IsManageMode)
            {
                Dictionary<string, SkillLoadoutItemConfigurationV3> loadoutConfig = GlobalData.Instance.Configuration.SkillLoadouts;

                loadoutConfig.Clear();
                foreach (LoadoutViewModel x in loadoutSelectorViewModel.Loadouts)
                {
                    loadoutConfig[x.Name] = new SkillLoadoutItemConfigurationV3
                    {
                        WeaponSlots = x.WeaponSlots,
                        Skills = x.Abilities
                            .Select(item => new SkillLoadoutItemConfigurationV2
                            {
                                SkillName = Core.Localization.GetDefault(item.SkillName),
                                Level = item.Level
                            })
                            .ToArray()
                    };
                }

                if (currentLoadout != null)
                    CurrentLoadoutName = currentLoadout.Name;
            }
        }
    }
}
