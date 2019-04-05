using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core.DataStructures;
using MHArmory.Search;
using MHArmory.Search.Contracts;
using MHArmory.Core.WPF;
using MHArmory.Core;

namespace MHArmory.ViewModels
{
    public class AdvancedSearchEquipment : ViewModelBase, ISolverDataEquipmentModel
    {
        public bool IsSelected
        {
            get { return model.IsSelected; }
            set
            {
                if (model.IsSelected != value)
                {
                    model.IsSelected = value;
                    onSelectionChanged();
                    NotifyPropertyChanged();
                }
            }
        }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        public IEquipment Equipment { get { return model.Equipment; } }

        private readonly ISolverDataEquipmentModel model;
        private readonly Action onSelectionChanged;
        private readonly bool originalSelection;

        public AdvancedSearchEquipment(ISolverDataEquipmentModel model, Action onSelectionChanged)
        {
            this.model = model;
            this.onSelectionChanged = onSelectionChanged;
            originalSelection = model.IsSelected;
        }

        public void RestoreOriginalSelection()
        {
            IsSelected = originalSelection;
        }
    }

    public class ArmorPieceTypesViewModel : ViewModelBase
    {
        public IList<AdvancedSearchEquipment> Equipments { get; private set; }

        public EquipmentType Type { get; }

        private int selectedCount;
        public int SelectedCount
        {
            get { return selectedCount; }
            private set { SetValue(ref selectedCount, value); }
        }

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

        public ICommand SelectAllCommand { get; }
        public ICommand UnselectAllCommand { get; }
        public ICommand InverseSelectionCommand { get; }
        public ICommand ResetSelectionCommand { get; }

        private readonly RootViewModel root;

        public ArmorPieceTypesViewModel(RootViewModel root, EquipmentType type, IEnumerable<ISolverDataEquipmentModel> equipments)
        {
            this.root = root;

            Type = type;
            Equipments = equipments
                .OrderBy(x => x.Equipment.Id)
                .Select(x => new AdvancedSearchEquipment(x, UpdateSelectedCount))
                .ToList();

            SelectAllCommand = new AnonymousCommand(OnSelectAll);
            UnselectAllCommand = new AnonymousCommand(OnUnselectAll);
            InverseSelectionCommand = new AnonymousCommand(OnInverseSelection);
            ResetSelectionCommand = new AnonymousCommand(OnResetSelection);

            UpdateSelectedCount();
        }

        private void OnSearchTextChanged()
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (AdvancedSearchEquipment x in Equipments)
                    x.IsVisible = true;
            }
            else
            {
                var searchStatement = SearchStatement.Create(searchText, GlobalData.Instance.Aliases);

                foreach (AdvancedSearchEquipment x in Equipments)
                    x.IsVisible = searchStatement.IsMatching(Localization.Get(x.Equipment.Name));
            }
        }

        private bool isInBatch;

        private void UpdateSelectedCount()
        {
            if (isInBatch)
                return;

            SelectedCount = Equipments.Count(x => x.IsSelected);
            root.UpdateMetrics();
        }

        private void OnSelectAll(object parameter)
        {
            isInBatch = true;

            try
            {
                foreach (AdvancedSearchEquipment x in Equipments.Where(x => x.IsVisible))
                    x.IsSelected = true;
            }
            finally
            {
                isInBatch = false;
                UpdateSelectedCount();
            }
        }

        private void OnUnselectAll(object parameter)
        {
            isInBatch = true;

            try
            {
                foreach (AdvancedSearchEquipment x in Equipments.Where(x => x.IsVisible))
                x.IsSelected = false;
            }
            finally
            {
                isInBatch = false;
                UpdateSelectedCount();
            }
        }

        private void OnInverseSelection(object parameter)
        {
            isInBatch = true;

            try
            {
                foreach (AdvancedSearchEquipment x in Equipments.Where(x => x.IsVisible))
                x.IsSelected = !x.IsSelected;
            }
            finally
            {
                isInBatch = false;
                UpdateSelectedCount();
            }
        }

        private void OnResetSelection(object parameter)
        {
            isInBatch = true;

            try
            {
                foreach (AdvancedSearchEquipment x in Equipments.Where(x => x.IsVisible))
                x.RestoreOriginalSelection();
            }
            finally
            {
                isInBatch = false;
                UpdateSelectedCount();
            }
        }
    }
}
