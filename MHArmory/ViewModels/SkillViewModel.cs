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
        private readonly IAbility ability;
        private readonly SkillViewModel parent;

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (SetValue(ref isChecked, value))
                    parent.CheckChanged(Level, isChecked);
            }
        }

        public int Id => ability.Id;
        public int SkillId => ability.Skill.Id;
        public string SkillName => ability.Skill.Name;
        public int Level => ability.Level;
        public string Description => ability.Description;

        public AbilityViewModel(IAbility ability, SkillViewModel parent)
        {
            this.ability = ability;
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
            IsVisible = ability.Level >= minVisibleLevel;
        }
    }

    public class SkillViewModel : ViewModelBase
    {
        private readonly ISkill skill;
        private readonly IList<IJewel> jewels;
        private readonly RootViewModel root;
        private readonly SkillSelectorViewModel skillSelector;

        public string Name => skill.Name;
        public string Description => skill.Description;

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
                JewelsText = $"({string.Join(", ", jewels.Select(x => x.Name))})";

            Abilities = skill.Abilities
                .OrderBy(x => x.Level)
                .Select(x => new AbilityViewModel(x, this))
                .ToList();
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

        public void ApplySearchText(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                IsVisible = true;
                return;
            }

            IsVisible =
                skill.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1 ||
                skill.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1 ||
                skill.Abilities.Any(x => x.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1) ||
                jewels.Any(j => j.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1);
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

            root.SearchArmorSets();
            skillSelector?.ComputeVisibility(this);
        }
    }
}
