using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.ViewModels
{
    public class AdvancedSearchViewModel : ViewModelBase
    {
        private ArmorPieceTypesViewModel[] armorPieceTypes;
        public ArmorPieceTypesViewModel[] ArmorPieceTypes
        {
            get { return armorPieceTypes; }
            private set { SetValue(ref armorPieceTypes, value); }
        }

        public void Update(ArmorPieceTypesViewModel[] armorPieceTypes)
        {
            if (ArmorPieceTypes != null)
            {
                foreach (ArmorPieceTypesViewModel x in ArmorPieceTypes)
                    x.Dispose();
            }

            ArmorPieceTypes = armorPieceTypes;
        }
    }
}
