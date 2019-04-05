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
    public class JewelAbilityViewModel : ViewModelBase
    {
        public Dictionary<string, string> SkillName { get; }
        public int MaxLevel { get; }

        private readonly Func<FullSkillDescriptionViewModel> descriptionFunc;
        private FullSkillDescriptionViewModel description;
        public FullSkillDescriptionViewModel Description
        {
            get
            {
                if (description == null)
                    description = descriptionFunc();
                return description;
            }
        }

        public JewelAbilityViewModel(IAbility ability, int level)
        {
            SkillName = ability.Skill.Name;
            MaxLevel = ability.Skill.MaxLevel;
            descriptionFunc = () => new FullSkillDescriptionViewModel(ability.Skill, level);
        }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }
    }

    public class JewelOverrideViewModel : ViewModelBase
    {
        private readonly DecorationsOverrideViewModel parent;
        private readonly IJewel jewel;

        public Dictionary<string, string> Name { get; }
        public int SlotSize { get; }
        public IList<JewelAbilityViewModel> Abilities { get; }

        private bool hasAllJewels = false;
        public bool HasAllJewels
        {
            get { return hasAllJewels; }
            private set { SetValue(ref hasAllJewels, value); }
        }

        private bool hasTooManyJewels = false;
        public bool HasTooManyJewels
        {
            get { return hasTooManyJewels; }
            private set { SetValue(ref hasTooManyJewels, value); }
        }

        public bool CanReportStateChange { get; set; } = true;

        private int count;
        public int Count
        {
            get { return count; }
            set
            {
                int originalValue = count;

                if (SetValue(ref count, value))
                {
                    SetValue(ref count, Math.Max(0, value));

                    HasAllJewels = Abilities.All(x => count == x.MaxLevel);
                    HasTooManyJewels = Abilities.All(x => count > x.MaxLevel);

                    foreach (JewelAbilityViewModel ability in Abilities)
                        ability.Description.UpdateLevel(count);

                    if (originalValue != count && CanReportStateChange)
                    {
                        parent.ComputeVisibility(this);
                        parent.StateChanged();
                    }
                }
            }
        }

        private bool isOverriding;
        public bool IsOverriding
        {
            get { return isOverriding; }
            set
            {
                if (SetValue(ref isOverriding, value) && CanReportStateChange)
                {
                    parent.ComputeVisibility(this);
                    parent.StateChanged();
                }
            }
        }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        public JewelOverrideViewModel(DecorationsOverrideViewModel parent, IJewel jewel, int count)
        {
            this.parent = parent;
            this.jewel = jewel;

            Name = jewel.Name;
            SlotSize = jewel.SlotSize;
            Abilities = jewel.Abilities.Select(x => new JewelAbilityViewModel(x, count)).ToList();
        }

        public void ApplySearchText(SearchStatement searchStatement)
        {
            if (searchStatement == null || searchStatement.IsEmpty)
            {
                IsVisible = true;
                return;
            }

            IsVisible =
                searchStatement.IsMatching(Localization.Get(jewel.Name)) ||
                jewel.Abilities.Any(x => searchStatement.IsMatching(Localization.Get(x.Skill.Name)));
        }
    }
}
