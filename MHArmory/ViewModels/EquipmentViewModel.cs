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
        public int Id { get { return Equipment.Id; } }
        public EquipmentType Type { get { return Equipment.Type; } }
        public string Name { get { return Equipment.Name; } } // TODO: localization here
        public int Rarity { get { return Equipment.Rarity; } }
        public int[] Slots { get { return Equipment.Slots; } }
        public IEvent Event { get { return Equipment.Event; } }
        public IAbility[] Abilities { get { return Equipment.Abilities; } }

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
