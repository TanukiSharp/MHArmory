using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Core.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.ViewModels
{
    public class ArmorSetSkillViewModel : ViewModelBase
    {
        private readonly IArmorSetSkillPart skillPart;
        private readonly ArmorSetBonusViewModel parent;

        public int RequiredArmorPieces {  get { return skillPart.RequiredArmorPieces; } }
        public IAbility[] GrantedSkills { get { return skillPart.GrantedSkills; } }

        public ArmorSetSkillViewModel(IArmorSetSkillPart skillPart, ArmorSetBonusViewModel parent)
        {
            this.skillPart = skillPart;
            this.parent = parent;
        }
    }

    public class ArmorSetBonusViewModel : ViewModelBase
    {
        private readonly RootViewModel root;
        private readonly ArmorSetBonusSelectorViewModel armorSetBonusSelectorViewModel;

        public IArmorSetSkill armorSetSkill { get; }
        public Dictionary<string, string> Name { get { return armorSetSkill.Name; } }
        public IList<ArmorSetSkillViewModel> Parts { get; }

        public ArmorSetBonusViewModel(IArmorSetSkill armorSetSkill, RootViewModel root, ArmorSetBonusSelectorViewModel armorSetBonusSelectorViewModel)
        {
            this.armorSetSkill = armorSetSkill;
            this.root = root;
            this.armorSetBonusSelectorViewModel = armorSetBonusSelectorViewModel;

            Parts = armorSetSkill.Parts
                .OrderBy(x => x.RequiredArmorPieces)
                .Select(x => new ArmorSetSkillViewModel(x, this))
                .ToList();
        }

        private bool isChecked = false;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (SetValue(ref isChecked, value))
                    root.SelectedWeaponSetBonusChanged();
            }
        }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        public void ApplySearchText(SearchStatement searchStatement)
        {
            if (searchStatement == null || searchStatement.IsEmpty)
            {
                IsVisible = true;
                return;
            }

            IsVisible =
                searchStatement.IsMatching(Localization.Get(armorSetSkill.Name)) ||
                armorSetSkill.Parts.Any(p => p.GrantedSkills.Any(x =>
                    searchStatement.IsMatching(Localization.Get(x.Skill.Name)) ||
                    searchStatement.IsMatching(Localization.Get(x.Skill.Description)) ||
                    searchStatement.IsMatching(Localization.Get(x.Description))));
        }
    }
}
