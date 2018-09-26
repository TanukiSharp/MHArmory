using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

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

        public int SkillId { get { return Ability.Skill.Id; } }
        public string SkillName { get { return Ability.Skill.Name; } }
        public int Level { get { return Ability.Level; } }
        public FullSkillDescriptionViewModel Description { get; }

        public AbilityViewModel(IAbility ability, SkillViewModel parent)
        {
            this.Ability = ability;
            this.parent = parent;

            Description = new FullSkillDescriptionViewModel(ability.Skill, ability.Level);
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

        public string Name { get { return skill.Name; } }
        public string Description { get { return skill.Description; } }

        public string JewelsText { get; private set; }

        public IList<AbilityViewModel> Abilities { get; }

        public SkillViewModel(ISkill skill, IList<IJewel> jewels, RootViewModel root, SkillSelectorViewModel skillSelector)
        {
            this.skill = skill;
            this.jewels = jewels;
            this.root = root;
            this.skillSelector = skillSelector;

            if (jewels == null || jewels.Count == 0)
                JewelsText = "(no jewel)";
            else
                JewelsText = $"({string.Join(", ", jewels.Select(x => $"{x.Name} [{x.SlotSize}]"))})";

            root.InParameters.PropertyChanged += InParameters_PropertyChanged;

            Abilities = skill.Abilities
                .OrderBy(x => x.Level)
                .Select(x => new AbilityViewModel(x, this))
                .ToList();

            UpdateAvailability();
        }

        private void InParameters_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InParametersViewModel.Rarity))
                UpdateAvailability();
        }

        private void UpdateAvailability()
        {
            if (jewels != null)
                IsAvailable = jewels.All(x => x.Rarity <= root.InParameters.Rarity);
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

        public void ApplySearchText(SearchStatement searchStatement)
        {
            if (searchStatement == null || searchStatement.IsEmpty)
            {
                IsVisible = true;
                return;
            }

            IsVisible =
                searchStatement.IsMatching(skill.Name) ||
                searchStatement.IsMatching(skill.Description) ||
                skill.Abilities.Any(x => searchStatement.IsMatching(x.Description)) ||
                searchStatement.IsMatching(JewelsText);
        }

        internal void CheckChanged(int level, bool resetChecked)
        {
            if (resetChecked)
            {
                foreach (AbilityViewModel vm in Abilities)
                {
                    if (vm.Level != level)
                        vm.IsChecked = false;
                }
            }

            root.CreateSolverData();

            if (root.IsAutoSearch)
                root.SearchArmorSets();

            skillSelector?.ComputeVisibility(this);

            root.SelectedAbilitiesChanged();
        }
    }
}
