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
        public ArmorPieceViewModel(IArmorPiece armorPiece)
            : base(armorPiece)
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
            set { SetValue(ref isPossessed, value); }
        }

        public ICommand TogglePossessionCommand { get; }

        private readonly IEquipment equipment;

        public EquipmentViewModel(IEquipment equipment)
        {
            this.equipment = equipment;

            TogglePossessionCommand = new AnonymousCommand(_ => IsPossessed = !IsPossessed);
        }
    }
}
