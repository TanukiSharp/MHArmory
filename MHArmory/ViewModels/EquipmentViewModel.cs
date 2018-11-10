using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class ArmorPieceViewModel : EquipmentViewModel
    {
        public ArmorPieceViewModel(RootViewModel rootViewModel, IArmorPiece armorPiece)
            : base(rootViewModel, armorPiece)
        {
        }
    }

    public class EquipmentViewModel : ViewModelBase, IEquipment
    {
        public int Id { get { return equipment.Id; } }
        public EquipmentType Type { get { return equipment.Type; } }
        public string Name { get { return equipment.Name; } } // TODO: localization here
        public int Rarity { get { return equipment.Rarity; } }
        public int[] Slots { get { return equipment.Slots; } }
        public IEvent Event { get { return equipment.Event; } }
        public IAbility[] Abilities { get { return equipment.Abilities; } }

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

        private readonly IEquipment equipment;

        private readonly RootViewModel rootViewModel;

        public EquipmentViewModel(RootViewModel rootViewModel, IEquipment equipment)
        {
            this.rootViewModel = rootViewModel;
            this.equipment = equipment;

            TogglePossessionCommand = new AnonymousCommand(_ => IsPossessed = !IsPossessed);
        }
    }
}
