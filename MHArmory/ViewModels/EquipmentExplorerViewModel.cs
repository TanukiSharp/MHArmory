using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public class EquipmentExplorerViewModel : ViewModelBase
    {
        public ObservableCollection<EquipmentViewModel> Equipments { get; } = new ObservableCollection<EquipmentViewModel>();

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (SetValue(ref searchText, value))
                    OnSearchTextChanged();
            }
        }

        private void OnSearchTextChanged()
        {
            Equipments.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (EquipmentViewModel x in rootViewModel.AllEquipments)
                    Equipments.Add(x);
            }
            else
            {
                var searchStatement = SearchStatement.Create(searchText);

                foreach (EquipmentViewModel x in rootViewModel.AllEquipments)
                {
                    bool isVisible = searchStatement.IsMatching(Localization.Get(x.Name)) ||
                        x.Abilities.Any(a => IsMatching(a, searchStatement));

                    if (isVisible)
                        Equipments.Add(x);
                }
            }
        }

        private static bool IsMatching(IAbility ability, SearchStatement searchStatement)
        {
            return searchStatement.IsMatching(Localization.Get(ability.Skill.Name)) ||
                searchStatement.IsMatching(Localization.Get(ability.Description)) ||
                searchStatement.IsMatching(Localization.Get(ability.Skill.Description));
        }

        public ICommand CancelCommand { get; }

        private readonly RootViewModel rootViewModel;

        public EquipmentExplorerViewModel(RootViewModel rootViewModel)
        {
            this.rootViewModel = rootViewModel;

            foreach (EquipmentViewModel equipment in rootViewModel.AllEquipments)
                Equipments.Add(equipment);

            CancelCommand = new AnonymousCommand(OnCancel);
        }

        private void OnCancel(object parameter)
        {
            if (parameter is CancellationCommandArgument cancellable)
            {
                if (string.IsNullOrWhiteSpace(SearchText) == false)
                {
                    SearchText = string.Empty;
                    cancellable.IsCancelled = true;
                }
            }
        }
    }
}
