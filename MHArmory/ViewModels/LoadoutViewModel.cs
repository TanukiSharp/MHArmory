using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MHArmory.ViewModels
{
    public class LoadoutViewModel : ViewModelBase
    {
        public string Name { get; }
        public AbilityViewModel[] Abilities { get; }

        public LoadoutViewModel(string name, AbilityViewModel[] abilities)
        {
            Name = name;
            Abilities = abilities;
        }
    }

    public class LoadoutSelectorViewModel : ViewModelBase
    {
        public LoadoutViewModel[] Loadout { get; }

        private LoadoutViewModel selectedLoadout;
        public LoadoutViewModel SelectedLoadout
        {
            get { return selectedLoadout; }
            set { SetValue(ref selectedLoadout, value); }
        }

        public ICommand AcceptCommand { get; }
        public ICommand CancelCommand { get; }

        private readonly Action<bool?> endFunc;
        private readonly IEnumerable<AbilityViewModel> abilities;

        public LoadoutSelectorViewModel(Action<bool?> endFunc, IEnumerable<AbilityViewModel> abilities)
        {
            this.endFunc = endFunc;
            this.abilities = abilities;

            AcceptCommand = new AnonymousCommand(OnAccept);
            CancelCommand = new AnonymousCommand(OnCancel);

            Dictionary<string, int[]> loadoutConfig = GlobalData.Instance?.Configuration?.Loadout;

            if (loadoutConfig == null)
                return;

            Loadout = loadoutConfig
                .Select(x => new LoadoutViewModel(x.Key, CreateAbilities(x.Value)))
                .ToArray();

            if (Loadout.Length > 1)
                SelectedLoadout = Loadout[1];
        }

        private AbilityViewModel[] CreateAbilities(int[] abilityIds)
        {
            var result = new List<AbilityViewModel>();

            foreach (int abilityId in abilityIds)
            {
                AbilityViewModel found = abilities.FirstOrDefault(a => a.Id == abilityId);
                if (found != null)
                    result.Add(found);
            }

            return result.ToArray();
        }

        private void OnAccept(object parameter)
        {
            endFunc(true);
        }

        private void OnCancel(object parameter)
        {
            SelectedLoadout = null;
            endFunc(false);
        }
    }
}
