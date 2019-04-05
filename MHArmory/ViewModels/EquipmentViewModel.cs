using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core.DataStructures;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public class ArmorPieceViewModel : EquipmentViewModel, IArmorPiece
    {
        public IArmorPieceDefense Defense { get { return ((IArmorPiece)Equipment).Defense; } }
        public IArmorPieceResistances Resistances { get { return ((IArmorPiece)Equipment).Resistances; } }
        public IArmorPieceAttributes Attributes { get { return ((IArmorPiece)Equipment).Attributes; } }
        public IArmorPieceAssets Assets { get { return ((IArmorPiece)Equipment).Assets; } }
        public IArmorSetSkill[] ArmorSetSkills { get { return ((IArmorPiece)Equipment).ArmorSetSkills; } }
        public IFullArmorSet FullArmorSet { get { return ((IArmorPiece)Equipment).FullArmorSet; } }

        public ArmorPieceViewModel(RootViewModel rootViewModel, IArmorPiece armorPiece)
            : base(rootViewModel, armorPiece)
        {
        }
    }

    public class EquipmentViewModel : ViewModelBase, IEquipment
    {
        public int Id { get { return Equipment.Id; } }
        public EquipmentType Type { get { return Equipment.Type; } }
        public Dictionary<string, string> Name { get { return Equipment.Name; } }
        public int Rarity { get { return Equipment.Rarity; } }
        public int[] Slots { get { return Equipment.Slots; } }
        public IEvent Event { get { return Equipment.Event; } }
        public IAbility[] Abilities { get { return Equipment.Abilities; } }
        public ICraftMaterial[] CraftMaterials { get { return Equipment.CraftMaterials; } }

        // This is used for the view, other types without see property
        // are considered not displaying the possessed mark.
        public bool ShowIsPossessed { get; } = true;

        private bool isPossessed = true;
        public bool IsPossessed
        {
            get { return isPossessed; }
            set
            {
                if (SetValue(ref isPossessed, value))
                    rootViewModel.EquipmentOverride.ComputeVisibility();
            }
        }

        public ICommand TogglePossessionCommand { get; }

        public IEquipment Equipment { get; private set; }

        private readonly RootViewModel rootViewModel;

        public EquipmentViewModel(RootViewModel rootViewModel, IEquipment equipment)
        {
            this.rootViewModel = rootViewModel;
            this.Equipment = equipment;

            TogglePossessionCommand = new AnonymousCommand(_ => IsPossessed = !IsPossessed);
        }
    }
}
