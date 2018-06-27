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
                if (SetValue(ref isChecked, value) && isChecked)
                    parent.CheckChanged(Level);
            }
        }

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

        public string Name => skill.Name;
        public string Description => skill.Description;

        public AbilityViewModel[] Abilities { get; }

        public SkillViewModel(ISkill skill)
        {
            this.skill = skill;

            Abilities = skill.Abilities
                .OrderBy(x => x.Level)
                .Select(x => new AbilityViewModel(x, this))
                .ToArray();
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
                skill.Abilities.Any(x => x.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1);
        }

        public void FilterLevel(int minVisibleLevel)
        {
            foreach (AbilityViewModel abilityViewModel in Abilities)
                abilityViewModel.FilterLevel(minVisibleLevel);
        }

        internal void CheckChanged(int level)
        {
            foreach (AbilityViewModel vm in Abilities)
            {
                if (vm.Level != level)
                    vm.IsChecked = false;
            }
        }
    }
}
