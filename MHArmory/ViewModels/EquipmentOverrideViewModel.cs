using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class EquipmentGroup
    {
        public string Name { get; }
        public EquipmentViewModel Head { get; }
        public EquipmentViewModel Chest { get; }
        public EquipmentViewModel Gloves { get; }
        public EquipmentViewModel Waist { get; }
        public EquipmentViewModel Legs { get; }

        public EquipmentGroup(IEnumerable<EquipmentViewModel> equipments)
            : this(
                  equipments.FirstOrDefault(x => x.Type == EquipmentType.Head),
                  equipments.FirstOrDefault(x => x.Type == EquipmentType.Chest),
                  equipments.FirstOrDefault(x => x.Type == EquipmentType.Gloves),
                  equipments.FirstOrDefault(x => x.Type == EquipmentType.Waist),
                  equipments.FirstOrDefault(x => x.Type == EquipmentType.Legs)
                  )
        {
        }

        public EquipmentGroup(
            EquipmentViewModel head,
            EquipmentViewModel chest,
            EquipmentViewModel gloves,
            EquipmentViewModel waist,
            EquipmentViewModel legs
        )
        {
            Head = head;
            Chest = chest;
            Gloves = gloves;
            Waist = waist;
            Legs = legs;

            Name = FindGroupName(head, chest, gloves, waist, legs);
        }

        public static string FindGroupName(params IEquipment[] equipments)
        {
            if (equipments == null || equipments.Length == 0)
                return null;

            string baseName = null;
            int firstPartMinLength = 0;
            int lastPartMinLength = 0;

            for (int i = 0; i < equipments.Length; i++)
            {
                if (equipments[i] == null)
                    continue;

                if (baseName == null)
                {
                    baseName = equipments[i].Name;
                    firstPartMinLength = baseName.Length;
                    lastPartMinLength = baseName.Length;
                    continue;
                }

                int c;
                string name = equipments[i].Name;

                for (c = 0; c < name.Length && c < baseName.Length; c++)
                {
                    if (name[c] != baseName[c])
                        break;
                }

                if (c < firstPartMinLength)
                    firstPartMinLength = c;

                for (c = 0; c < name.Length && c < baseName.Length; c++)
                {
                    if (name[name.Length - c - 1] != baseName[baseName.Length - c - 1])
                        break;
                }

                if (c < lastPartMinLength)
                    lastPartMinLength = c;
            }

            if (firstPartMinLength == 0)
                return null;

            if (lastPartMinLength == 0 || firstPartMinLength == lastPartMinLength)
            {
                if (firstPartMinLength == baseName.Length)
                    return baseName;

                return baseName.Substring(0, firstPartMinLength).Trim();
            }

            string firstPart = baseName.Substring(0, firstPartMinLength).Trim();
            string lastPart = baseName.Substring(baseName.Length - lastPartMinLength).Trim();

            return $"{firstPart} {lastPart}";
        }
    }

    public class EquipmentOverrideViewModel : ViewModelBase
    {
        private readonly RootViewModel rootViewModel;

        private IList<EquipmentGroup> armorSets;
        public IList<EquipmentGroup> ArmorSets
        {
            get { return armorSets; }
            private set { SetValue(ref armorSets, value); }
        }

        public EquipmentOverrideViewModel(RootViewModel rootViewModel)
        {
            this.rootViewModel = rootViewModel;
        }

        internal void NotifyDataLoaded()
        {
            IEnumerable<IArmorPiece> allArmoprPieces =
                GlobalData.Instance.Heads
                .Concat(GlobalData.Instance.Chests)
                .Concat(GlobalData.Instance.Gloves)
                .Concat(GlobalData.Instance.Waists)
                .Concat(GlobalData.Instance.Legs);

            ArmorSets = allArmoprPieces
                .GroupBy(x => x.Id, x => new EquipmentViewModel(x))
                .Select(x => new EquipmentGroup(x))
                .OrderBy(x => x.Name)
                .ToList();
        }
    }
}
