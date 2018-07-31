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
    public class LoadoutDialogResult
    {
        public bool? Result { get; private set; }
        public LoadoutViewModel SelectedLoadout { get; private set; }

        public void Update(bool? result, LoadoutViewModel selectedLoadout)
        {
            Result = result;
            SelectedLoadout = selectedLoadout;
        }
    }

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

        public LoadoutWindow(IEnumerable<AbilityViewModel> abilities)
        {
            InitializeComponent();

            loadoutSelectorViewModel = new LoadoutSelectorViewModel(OnEnd, abilities);

            InputBindings.Add(new KeyBinding(loadoutSelectorViewModel.AcceptCommand, new KeyGesture(Key.Enter, ModifierKeys.None)));
            InputBindings.Add(new KeyBinding(loadoutSelectorViewModel.CancelCommand, new KeyGesture(Key.Escape, ModifierKeys.None)));

            DataContext = loadoutSelectorViewModel;
        }

        private void OnEnd(bool? value)
        {
            DialogResult = value;
        }
    }
}
