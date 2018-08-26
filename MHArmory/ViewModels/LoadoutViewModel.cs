using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Services;

namespace MHArmory.ViewModels
{
    public class LoadoutViewModel : ViewModelBase
    {
        private readonly LoadoutSelectorViewModel parent;

        public bool IsManageMode { get; }

        public ICommand RenameCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand DeleteCommand { get; }

        private string name;
        public string Name
        {
            get { return name; }
            set { SetValue(ref name, value); }
        }

        public AbilityViewModel[] Abilities { get; }

        public LoadoutViewModel(bool isManageMode, string name, AbilityViewModel[] abilities, LoadoutSelectorViewModel parent)
        {
            this.parent = parent;

            IsManageMode = isManageMode;

            Name = name;
            Abilities = abilities;

            RenameCommand = new AnonymousCommand(OnRename);
            MoveUpCommand = new AnonymousCommand(OnMoveUp);
            MoveDownCommand = new AnonymousCommand(OnMoveDown);
            DeleteCommand = new AnonymousCommand(OnDelete);
        }

        private void OnRename()
        {
            parent.RenameLoadout(this);
        }

        private void OnMoveUp()
        {
            parent.MoveLoadoutUp(this);
        }

        private void OnMoveDown()
        {
            parent.MoveLoadoutDown(this);
        }

        private void OnDelete()
        {
            parent.DeleteLoadout(this);
        }
    }

    public class LoadoutSelectorViewModel : ViewModelBase
    {
        public ObservableCollection<LoadoutViewModel> Loadouts { get; }

        private LoadoutViewModel selectedLoadout;
        public LoadoutViewModel SelectedLoadout
        {
            get { return selectedLoadout; }
            set { SetValue(ref selectedLoadout, value); }
        }

        public ICommand AcceptCommand { get; }
        public ICommand CancelCommand { get; }

        public bool IsManageMode { get; }

        private readonly Action<bool?> endFunc;
        private readonly IEnumerable<AbilityViewModel> abilities;

        public LoadoutSelectorViewModel(bool isManageMode, Action<bool?> endFunc, IEnumerable<AbilityViewModel> abilities)
        {
            IsManageMode = isManageMode;

            this.endFunc = endFunc;
            this.abilities = abilities;

            AcceptCommand = new AnonymousCommand(OnAccept);
            CancelCommand = new AnonymousCommand(OnCancel);

            Dictionary<string, int[]> loadoutConfig = GlobalData.Instance?.Configuration?.SkillLoadouts;

            if (loadoutConfig == null)
                return;

            Loadouts = new ObservableCollection<LoadoutViewModel>(
                loadoutConfig.Select(x => new LoadoutViewModel(isManageMode, x.Key, CreateAbilities(x.Value), this))
            );

            if (Loadouts.Count > 1)
                SelectedLoadout = Loadouts[1];
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

        public bool RenameLoadout(LoadoutViewModel loadoutViewModel)
        {
            var renameOptions = new RenameOptions
            {
                WindowTitle = $"Rename skill loadout",
                WindowPrompt = $"Rename skill loadout '{loadoutViewModel.Name}'",
                WindowDefaultValue = loadoutViewModel.Name,
                IsInputMandatory = true,
                IsValid = x => Loadouts.Where(l => l != loadoutViewModel).All(l => l.Name != x)
            };

            if (RenameService.Rename(renameOptions, out string newName))
            {
                loadoutViewModel.Name = newName;
                return true;
            }

            return false;
        }

        public bool MoveLoadoutUp(LoadoutViewModel loadout)
        {
            return ReorganizableCollectionUtilities<LoadoutViewModel>.MoveUp(
                Loadouts,
                loadout,
                () => SelectedLoadout,
                x => SelectedLoadout = x
            );
        }

        public bool MoveLoadoutDown(LoadoutViewModel loadout)
        {
            return ReorganizableCollectionUtilities<LoadoutViewModel>.MoveDown(
                Loadouts,
                loadout,
                () => SelectedLoadout,
                x => SelectedLoadout = x
            );
        }

        public bool DeleteLoadout(LoadoutViewModel loadout)
        {
            return ReorganizableCollectionUtilities<LoadoutViewModel>.Remove(
                Loadouts,
                loadout,
                x => SelectedLoadout == x,
                () => SelectedLoadout = null
            );
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
