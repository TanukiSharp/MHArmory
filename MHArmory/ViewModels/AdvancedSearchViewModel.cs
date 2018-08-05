using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.ViewModels
{
    public class AdvancedSearchViewModel : ViewModelBase, IDisposable
    {
        public ArmorPieceTypesViewModel[] ArmorPieceTypes { get; }

        public AdvancedSearchViewModel(ArmorPieceTypesViewModel[] armorPieceTypes)
        {
            ArmorPieceTypes = armorPieceTypes;
        }

        public void Dispose()
        {
            foreach (ArmorPieceTypesViewModel x in ArmorPieceTypes)
                x.Dispose();
        }
    }
}
