using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class ArmorSetSkillPartViewModel : ViewModelBase, IArmorSetSkillPart
    {
        public int Id { get { return armorSetSkillPart.Id; } }
        public int RequiredArmorPieces { get { return armorSetSkillPart.RequiredArmorPieces; } }

        private IAbility[] grantedSkills;
        public IAbility[] GrantedSkills
        {
            get
            {
                LazyCreateGrantedSkills();
                return grantedSkills;
            }
        }

        private readonly IArmorSetSkillPart armorSetSkillPart;

        public ArmorSetSkillPartViewModel(IArmorSetSkillPart armorSetSkillPart)
        {
            this.armorSetSkillPart = armorSetSkillPart;
        }

        private void LazyCreateGrantedSkills()
        {
            if (grantedSkills == null)
                grantedSkills = armorSetSkillPart.GrantedSkills.Select(x => new AbilityViewModel(x, null)).ToArray();
        }
    }

    public class ArmorSetSkillViewModel : ViewModelBase, IArmorSetSkill
    {
        public int Id { get { return armorSetSkill.Id; } }
        public Dictionary<string, string> Name { get { return armorSetSkill.Name; } }

        private IArmorSetSkillPart[] parts;
        public IArmorSetSkillPart[] Parts
        {
            get
            {
                LazyCreateParts();
                return parts;
            }
        }

        private readonly IArmorSetSkill armorSetSkill;

        public ArmorSetSkillViewModel(IArmorSetSkill armorSetSkill)
        {
            this.armorSetSkill = armorSetSkill;
        }

        private void LazyCreateParts()
        {
            if (parts == null)
                parts = armorSetSkill.Parts.Select(x => new ArmorSetSkillPartViewModel(x)).ToArray();
        }
    }

    public class ArmorPieceViewModel : EquipmentViewModel, IArmorPiece
    {
        public IArmorPieceDefense Defense { get { return ((IArmorPiece)Equipment).Defense; } }
        public IArmorPieceResistances Resistances { get { return ((IArmorPiece)Equipment).Resistances; } }
        public IArmorPieceAttributes Attributes { get { return ((IArmorPiece)Equipment).Attributes; } }
        public IArmorPieceAssets Assets { get { return ((IArmorPiece)Equipment).Assets; } }

        private IArmorSetSkill[] armorSetSkills;
        public IArmorSetSkill[] ArmorSetSkills
        {
            get
            {
                LazyCreateArmorSetSkills();
                return armorSetSkills;
            }
        }

        public IFullArmorSet FullArmorSet { get { return ((IArmorPiece)Equipment).FullArmorSet; } }

        public ArmorPieceViewModel(RootViewModel rootViewModel, IArmorPiece armorPiece)
            : base(rootViewModel, armorPiece)
        {
        }

        private void LazyCreateArmorSetSkills()
        {
            if (armorSetSkills == null && ((IArmorPiece)Equipment).ArmorSetSkills != null)
                armorSetSkills = ((IArmorPiece)Equipment).ArmorSetSkills.Select(x => new ArmorSetSkillViewModel(x)).ToArray();
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
