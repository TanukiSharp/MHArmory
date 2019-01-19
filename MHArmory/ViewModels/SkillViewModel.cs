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

        private static readonly Dictionary<string, string> ExcludeText = new Dictionary<string, string> { [Localization.DefaultLanguage] = "Exclude" };

        public SkillViewModel(ISkill skill, IList<IJewel> jewels, RootViewModel root, SkillSelectorViewModel skillSelector)
        {
            this.skill = skill;
            this.jewels = jewels;
            this.root = root;
            this.skillSelector = skillSelector;

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
                searchStatement.IsMatching(Localization.Get(skill.Name)) ||
                searchStatement.IsMatching(Localization.Get(skill.Description)) ||
                skill.Abilities.Any(x => searchStatement.IsMatching(Localization.Get(x.Description))) ||
                searchStatement.IsMatching(JewelsText);
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
