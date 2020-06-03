using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public class AbilityViewModel : ViewModelBase
    {
        public readonly IAbility Ability;
        private readonly SkillViewModel parent;

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (SetValue(ref isChecked, value))
                    parent?.CheckChanged(Level, isChecked);
            }
        }

        private bool isHidden;
        public bool IsHidden
        {
            get { return isHidden; }
            internal set { SetValue(ref isHidden, value); }
        }

        public int SkillId { get { return Ability.Skill.Id; } }
        public Dictionary<string, string> SkillName { get { return Ability.Skill.Name; } }
        public int Level { get { return Ability.Level; } }
        public Dictionary<string, string> AbilityDescription { get { return Ability.Description; } }

        private FullSkillDescriptionViewModel description;
        public FullSkillDescriptionViewModel Description
        {
            get
            {
                if (description == null)
                    description = new FullSkillDescriptionViewModel(Ability.Skill, Ability.Level);
                return description;
            }
        }

        public AbilityViewModel(IAbility ability, SkillViewModel parent)
        {
            Ability = ability;
            this.parent = parent;
        }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        public void FilterLevel(int minVisibleLevel)
        {
            IsVisible = Ability.Level >= minVisibleLevel;
        }
    }

    public class SkillViewModel : ViewModelBase
    {
        private readonly ISkill skill;
        private readonly IList<IJewel> jewels;
        private readonly RootViewModel root;
        private readonly SkillSelectorViewModel skillSelector;

        public Dictionary<string, string> Name { get { return skill.Name; } }
        public Dictionary<string, string> Description { get { return skill.Description; } }

        private string jewelsText;
        public string JewelsText
        {
            get { return jewelsText; }
            private set { SetValue(ref jewelsText, value); }
        }

        public IList<AbilityViewModel> Abilities { get; }
        public IList<string> Categories { get; }

        private static readonly Dictionary<string, string> ExcludeText = new Dictionary<string, string> { [Localization.DefaultLanguage] = "Exclude" };

        public SkillViewModel(ISkill skill, IList<IJewel> jewels, RootViewModel root, SkillSelectorViewModel skillSelector)
        {
            this.skill = skill;
            this.jewels = jewels;
            this.root = root;
            this.skillSelector = skillSelector;

            Categories = skill.Categories;

            UpdateJewelsText();

            Localization.RegisterLanguageChanged(this, self =>
            {
                ((SkillViewModel)self).UpdateJewelsText();
            });

            root.InParameters.PropertyChanged += InParameters_PropertyChanged;

            Abilities = skill.Abilities
                .Where(x => x.Level > 0)
                .OrderBy(x => x.Level)
                .Select(x => new AbilityViewModel(x, this))
                .ToList();

            // Insert the "skill exlusion" item at position 0.
            Abilities.Insert(0, new AbilityViewModel(new Ability(skill, 0, ExcludeText), this));

            UpdateAvailability();
        }

        private void UpdateJewelsText()
        {
            if (jewels == null || jewels.Count == 0)
                JewelsText = "(no jewel)";
            else
                JewelsText = $"({string.Join(", ", jewels.Select(x => $"{Localization.Get(x.Name)} [{x.SlotSize}]"))})";
        }

        private void InParameters_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InParametersViewModel.Rarity))
                UpdateAvailability();
        }

        private void UpdateAvailability()
        {
            isAvailable = false;
            if (jewels != null && jewels.Count > 0)
                IsAvailable |= jewels.Any(x => x.Rarity <= root.InParameters.Rarity);
            if (GlobalData.Instance.Charms != null) 
                IsAvailable |= GlobalData.Instance.Charms.Where(c => c.Abilities.Any(a => a.Skill == skill)).Any(c => c.Rarity <= root.InParameters.Rarity);
            bool CheckSkillOnEquipment(IList<IArmorPiece> equipment)
            {
                if (equipment == null)
                    return false;
                bool CheckCompatibility(IArmorPiece armorPiece)
                {
                    if (armorPiece.Abilities.Any(a => a.Skill == skill))
                        return true;
                    if(armorPiece.ArmorSetSkills != null)
                    {
                        foreach (IArmorSetSkill setSkill in armorPiece.ArmorSetSkills)
                        {
                            foreach (IArmorSetSkillPart part in setSkill.Parts)
                            {
                                if (part.GrantedSkills.Any(a => a.Skill == skill))
                                    return true;
                            }
                        }
                    }
                    return false;
                }
                return equipment.Where(x => CheckCompatibility(x)).Any(x => x.Rarity <= root.InParameters.Rarity);
            }
            IsAvailable |= CheckSkillOnEquipment(GlobalData.Instance.Heads);
            IsAvailable |= CheckSkillOnEquipment(GlobalData.Instance.Chests);
            IsAvailable |= CheckSkillOnEquipment(GlobalData.Instance.Gloves);
            IsAvailable |= CheckSkillOnEquipment(GlobalData.Instance.Waists);
            IsAvailable |= CheckSkillOnEquipment(GlobalData.Instance.Legs);
        }

        public bool HasCheckedAbility
        {
            get
            {
                return Abilities.Any(x => x.IsChecked);
            }
        }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        private bool isAvailable = true;
        public bool IsAvailable
        {
            get { return isAvailable; }
            set { SetValue(ref isAvailable, value); }
        }

        // This variable is set to a 0 or higher value if it is subject to be set by search text, -1 otherwise.
        private int searchTextSkillLevel = -1;

        public void ApplySearchText(SearchStatement searchStatement, int? numercModifier)
        {
            searchTextSkillLevel = -1;

            if (searchStatement == null || searchStatement.IsEmpty)
            {
                IsVisible = true;
                return;
            }

            IsVisible =
                searchStatement.IsMatching(Localization.Get(skill.Name)) ||
                searchStatement.IsMatching(Localization.Get(skill.Description)) ||
                skill.Abilities.Any(x => searchStatement.IsMatching(Localization.Get(x.Description))) ||
                searchStatement.IsMatching(JewelsText);

            if (IsVisible)
            {
                if (numercModifier.HasValue == false)
                {
                    foreach (AbilityViewModel x in Abilities)
                        x.IsHidden = false;
                }
                else
                {
                    if (numercModifier.Value < 0 || numercModifier.Value > skill.MaxLevel)
                        IsVisible = false;
                    else
                    {
                        searchTextSkillLevel = numercModifier.Value;
                        foreach (AbilityViewModel x in Abilities)
                            x.IsHidden = x.Level != numercModifier.Value;
                    }
                }
            }
        }

        public void ApplySearchTextSkillLevel()
        {
            if (searchTextSkillLevel < 0)
                return;

            if (searchTextSkillLevel <= Abilities.Count)
            {
                Abilities[searchTextSkillLevel].IsChecked = !Abilities[searchTextSkillLevel].IsChecked;
                CheckChanged(searchTextSkillLevel, true);
            }
        }

        private bool isCheckChanging = false;

        internal void CheckChanged(int level, bool resetChecked)
        {
            if (isCheckChanging)
                return;

            isCheckChanging = true;

            if (resetChecked)
            {
                foreach (AbilityViewModel vm in Abilities)
                {
                    if (vm.Level != level)
                        vm.IsChecked = false;
                }
            }

            isCheckChanging = false;

            skillSelector?.ComputeVisibility(this);

            root.SelectedAbilitiesChanged();
        }
    }
}
