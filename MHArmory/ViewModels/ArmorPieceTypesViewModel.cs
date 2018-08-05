using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Search;

namespace MHArmory.ViewModels
{
    public class ArmorPieceTypesViewModel : ViewModelBase, IDisposable
    {
        public IEnumerable<SolverDataEquipmentModel> Equipments { get; }

        private int selectedCount;
        public int SelectedCount
        {
            get { return selectedCount; }
            private set { SetValue(ref selectedCount, value); }
        }

        public ArmorPieceTypesViewModel(IEnumerable<SolverDataEquipmentModel> equipments)
        {
            Equipments = equipments.OrderBy(x => x.Equipment.Name).ToList();

            foreach (SolverDataEquipmentModel x in Equipments)
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

        public void Dispose()
        {
            foreach (SolverDataEquipmentModel x in Equipments)
                x.SelectionChanged -= ItemSelectionChanged;
        }
    }
}
