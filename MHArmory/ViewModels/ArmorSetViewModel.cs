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
            private set { SetValue(ref jewel, value); }
        }

        private int count;
        public int Count
        {
            get { return count; }
            private set { SetValue(ref count, value); }
        }

        public ArmorSetJewelViewModel(IJewel jewel, int count)
        {
            this.jewel = jewel;
            this.count = count;
        }
    }

    public class ArmorSetViewModel : ViewModelBase
    {
        private IList<IArmorPiece> armorPieces;
        public IList<IArmorPiece> ArmorPieces
        {
            get { return armorPieces; }
            private set { SetValue(ref armorPieces, value); }
        }

        private ICharmLevel charm;
        public ICharmLevel Charm
        {
            get { return charm; }
            private set { SetValue(ref charm, value); }
        }

        private IList<ArmorSetJewelViewModel> jewels;
        public IList<ArmorSetJewelViewModel> Jewels
        {
            get { return jewels; }
            private set { SetValue(ref jewels, value); }
        }

        public int[] SpareSlots { get; }

        public int TotalBaseDefense { get; }
        public int TotalMaxDefense { get; }
        public int TotalAugmentedDefense { get; }

        public ArmorSetViewModel(IList<IArmorPiece> armorPieces, ICharmLevel charm, IList<ArmorSetJewelViewModel> jewels, int[] spareSlots)
        {
            this.armorPieces = armorPieces;
            this.charm = charm;
            this.jewels = jewels;

            SpareSlots = spareSlots;

            TotalBaseDefense = armorPieces.Sum(x => x.Defense.Base);
            TotalMaxDefense = armorPieces.Sum(x => x.Defense.Max);
            TotalAugmentedDefense = armorPieces.Sum(x => x.Defense.Augmented);
        }
    }
}
