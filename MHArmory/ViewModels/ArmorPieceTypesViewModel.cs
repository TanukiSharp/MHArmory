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

namespace MHArmory.ViewModels
{
    public class ArmorPieceTypesViewModel : ViewModelBase, IDisposable
    {
        public IList<ISolverDataEquipmentModel> Equipments { get; private set; }

        public EquipmentType Type { get; }

        private int selectedCount;
        public int SelectedCount
        {
            get { return selectedCount; }
            private set { SetValue(ref selectedCount, value); }
        }

        public ICommand SelectAllCommand { get; }
        public ICommand UnselectAllCommand { get; }
        public ICommand InverseSelectionCommand { get; }
        public ICommand ResetSelectionCommand { get; }

        public ArmorPieceTypesViewModel(EquipmentType type, IEnumerable<ISolverDataEquipmentModel> equipments)
        {
            Type = type;
            Equipments = equipments.OrderBy(x => x.Equipment.Id).ToList();

            SelectAllCommand = new AnonymousCommand(OnSelectAll);
            UnselectAllCommand = new AnonymousCommand(OnUnselectAll);
            InverseSelectionCommand = new AnonymousCommand(OnInverseSelection);
            ResetSelectionCommand = new AnonymousCommand(OnResetSelection);

            foreach (ISolverDataEquipmentModel x in Equipments)
                x.SelectionChanged += ItemSelectionChanged;

            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            SelectedCount = Equipments.Count(x => x.IsSelected);
        }

        private void ItemSelectionChanged(object sender, EventArgs e)
        {
            UpdateSelectedCount();
        }

        private void OnSelectAll(object parameter)
        {
            foreach (ISolverDataEquipmentModel x in Equipments)
                x.IsSelected = true;
            ForceRefreshEquipments();
        }

        private void OnUnselectAll(object parameter)
        {
            foreach (ISolverDataEquipmentModel x in Equipments)
                x.IsSelected = false;
            ForceRefreshEquipments();
        }

        private void OnInverseSelection(object parameter)
        {
            foreach (ISolverDataEquipmentModel x in Equipments)
                x.IsSelected = !x.IsSelected;
            ForceRefreshEquipments();
        }

        private void OnResetSelection(object parameter)
        {
            foreach (ISolverDataEquipmentModel x in Equipments)
                x.RestoreOriginalSelection();
            ForceRefreshEquipments();
        }

        internal void ForceRefreshEquipments()
        {
            IList<ISolverDataEquipmentModel> equipmentsReference = Equipments;
            Equipments = null;
            NotifyPropertyChanged(nameof(Equipments));
            Equipments = equipmentsReference;
            NotifyPropertyChanged(nameof(Equipments));
        }

        public void Dispose()
        {
            foreach (ISolverDataEquipmentModel x in Equipments)
                x.SelectionChanged -= ItemSelectionChanged;
        }
    }
}
