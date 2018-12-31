using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Core.Interfaces;

namespace MHArmory.ViewModels
{
    public class JewelAbilityViewModel : ViewModelBase, IDisposable
    {
        public Dictionary<string, string> SkillName { get { return ability.Skill.Name; } }

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

        private readonly IAbility ability;

        public JewelAbilityViewModel(IAbility ability, int level)
        {
            this.ability = ability;
            descriptionFunc = () => new FullSkillDescriptionViewModel(ability.Skill, level);

            Localization.LanguageChanged += Localization_LanguageChanged;
        }

        private void Localization_LanguageChanged(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(SkillName));
        }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        public void Dispose()
        {
            if (description != null)
                description.Dispose();

            Localization.LanguageChanged -= Localization_LanguageChanged;
        }
    }

    public class JewelOverrideViewModel : ViewModelBase, ICleanable
    {
        private readonly DecorationsOverrideViewModel parent;
        private readonly IJewel jewel;

        public Dictionary<string, string> Name { get { return jewel.Name; } }
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

            SlotSize = jewel.SlotSize;
            Abilities = jewel.Abilities.Select(x => new JewelAbilityViewModel(x, count)).ToList();

            Localization.LanguageChanged += Localization_LanguageChanged;
        }

        private void Localization_LanguageChanged(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(Name));
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

        public void Cleanup()
        {
            Localization.LanguageChanged += Localization_LanguageChanged;
        }
    }
}
