using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class ArmorSetJewelViewModel : ViewModelBase
    {
        private IJewel jewel;
        public IJewel Jewel
        {
            get { return jewel; }
            set { SetValue(ref jewel, value); }
        }

        private int count;
        public int Count
        {
            get { return count; }
            set { SetValue(ref count, value); }
        }
    }

    public class ArmorSetViewModel : ViewModelBase
    {
        private IList<IArmorPiece> armorPieces;
        public IList<IArmorPiece> ArmorPieces
        {
            get { return armorPieces; }
            set { SetValue(ref armorPieces, value); }
        }

        private ICharmLevel charm;
        public ICharmLevel Charm
        {
            get { return charm; }
            set { SetValue(ref charm, value); }
        }

        private IList<ArmorSetJewelViewModel> jewels;
        public IList<ArmorSetJewelViewModel> Jewels
        {
            get { return jewels; }
            set { SetValue(ref jewels, value); }
        }
    }
}
