using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class JewelAbilityViewModel : ViewModelBase
    {
        public string SkillName { get; }
        public FullSkillDescriptionViewModel Description { get; }

        public JewelAbilityViewModel(IAbility ability, int level)
        {
            SkillName = ability.Skill.Name;
            Description = new FullSkillDescriptionViewModel(ability.Skill, level);
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

        public string Name { get; }
        public int SlotSize { get; }
        public IList<JewelAbilityViewModel> Abilities { get; }

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
                searchStatement.IsMatching(jewel.Name) ||
                jewel.Abilities.Any(x => searchStatement.IsMatching(x.Skill.Name));
        }
    }
}
