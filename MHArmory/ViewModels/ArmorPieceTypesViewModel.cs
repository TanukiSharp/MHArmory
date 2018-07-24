using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Search;

namespace MHArmory.ViewModels
{
    public class ArmorPieceTypesViewModel : ViewModelBase
    {
        public IEnumerable<SolverDataEquipmentModel> Equipments { get; }

        public ArmorPieceTypesViewModel(IEnumerable<SolverDataEquipmentModel> equipments)
        {
            Equipments = equipments.OrderBy(x => x.Equipment.Name).ToList();
        }
    }
}
