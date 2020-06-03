using MHArmory.Core.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MHArmory.ViewModels
{
    public class ArmorSetBonusSelectorViewModel : ViewModelBase
    {
        public IEnumerable<ArmorSetBonusViewModel> SetBonuses { get; set; }

        public ICommand CancelCommand { get; }
        public ArmorSetBonusSelectorViewModel()
        {
            CancelCommand = new AnonymousCommand(OnCancel);
        }

        private VisibilityMode visibilityMode = VisibilityMode.All;

        public bool VisibilityModeAll
        {
            set
            {
                if (value && visibilityMode != VisibilityMode.All)
                {
                    visibilityMode = VisibilityMode.All;
                    ComputeVisibility();
                }
            }
        }

        public bool VisibilityModeSelected
        {
            set
            {
                if (value && visibilityMode != VisibilityMode.Selected)
                {
                    visibilityMode = VisibilityMode.Selected;
                    ComputeVisibility();
                }
            }
        }

        public bool VisibilityModeUnselected
        {
            set
            {
                if (value && visibilityMode != VisibilityMode.Unselected)
                {
                    visibilityMode = VisibilityMode.Unselected;
                    ComputeVisibility();
                }
            }
        }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (SetValue(ref searchText, value))
                {
                    OnSearchTextChanged();
                    if (SetBonuses != null)
                        ComputeVisibility();
                }
            }
        }

        private string searchTextPayload;
        private void OnSearchTextChanged()
        {
            searchTextPayload = searchText.Trim();
        }

        private void ComputeVisibility()
        {
            foreach (ArmorSetBonusViewModel vm in SetBonuses)
                ComputeVisibility(vm);
        }

        internal void ComputeVisibility(ArmorSetBonusViewModel armorSetBonusViewModel)
        {
            if (visibilityMode == VisibilityMode.Selected)
            {
                if (armorSetBonusViewModel.IsChecked == false)
                {
                    armorSetBonusViewModel.IsVisible = false;
                    return;
                }
            }
            else if (visibilityMode == VisibilityMode.Unselected)
            {
                if (armorSetBonusViewModel.IsChecked)
                {
                    armorSetBonusViewModel.IsVisible = false;
                    return;
                }
            }

            armorSetBonusViewModel.ApplySearchText(SearchStatement.Create(searchTextPayload, GlobalData.Instance.Aliases));
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
