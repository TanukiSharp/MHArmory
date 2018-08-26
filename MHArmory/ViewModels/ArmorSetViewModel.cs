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

        public int SpareSlotCount { get; }
        public int SpareSlotSizeSquare { get; }
        public int SpareSlotSizeCube { get; }

        public int TotalRarity { get; }

        public int TotalBaseDefense { get; }
        public int TotalMaxDefense { get; }
        public int TotalAugmentedDefense { get; }

        public int TotalFireResistance { get; }
        public int TotalWaterResistance { get; }
        public int TotalThunderResistance { get; }
        public int TotalIceResistance { get; }
        public int TotalDragonResistance { get; }

        public ArmorSetViewModel(IList<IArmorPiece> armorPieces, ICharmLevel charm, IList<ArmorSetJewelViewModel> jewels, int[] spareSlots)
        {
            this.armorPieces = armorPieces;
            this.charm = charm;
            this.jewels = jewels;

            SpareSlots = spareSlots;

            TotalRarity = armorPieces.Sum(x => x.Rarity);

            SpareSlotCount = SpareSlots.Count(x => x > 0);
            SpareSlotSizeSquare = Search.DataUtility.SlotSizeScoreSquare(SpareSlots);
            SpareSlotSizeCube = Search.DataUtility.SlotSizeScoreCube(SpareSlots);

            TotalBaseDefense = armorPieces.Sum(x => x?.Defense.Base ?? 0);
            TotalMaxDefense = armorPieces.Sum(x => x?.Defense.Max ?? 0);
            TotalAugmentedDefense = armorPieces.Sum(x => x?.Defense.Augmented ?? 0);

            TotalFireResistance = armorPieces.Sum(a => a.Resistances.Fire);
            TotalWaterResistance = armorPieces.Sum(a => a.Resistances.Water);
            TotalThunderResistance = armorPieces.Sum(a => a.Resistances.Thunder);
            TotalIceResistance = armorPieces.Sum(a => a.Resistances.Ice);
            TotalDragonResistance = armorPieces.Sum(a => a.Resistances.Dragon);
        }
    }
}
