using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
                CurrentLoadoutName = currentLoadout.Name;
                loadoutSelectorViewModel.SelectedLoadout = currentLoadout;
            }

            InputBindings.Add(new KeyBinding(loadoutSelectorViewModel.AcceptCommand, new KeyGesture(Key.Enter, ModifierKeys.None)));
            InputBindings.Add(new KeyBinding(loadoutSelectorViewModel.CancelCommand, new KeyGesture(Key.Escape, ModifierKeys.None)));

            DataContext = loadoutSelectorViewModel;
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
                Dictionary<string, int[]> loadoutConfig = GlobalData.Instance.Configuration.SkillLoadouts;

                loadoutConfig.Clear();
                foreach (LoadoutViewModel x in loadoutSelectorViewModel.Loadouts)
                    loadoutConfig[x.Name] = x.Abilities.Select(a => a.Id).ToArray();

                if (currentLoadout != null)
                    CurrentLoadoutName = currentLoadout.Name;
            }
        }
    }
}
