using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class JewelOverrideViewModel : ViewModelBase
    {
        private readonly DecorationsOverrideViewModel parent;
        private readonly IJewel jewel;

        public int Id { get; }
        public string Name { get; }
        public int SlotSize { get; }
        public IAbility[] Abilities { get; }

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

                    if (originalValue != count)
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
                if (SetValue(ref isOverriding, value))
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

            Id = jewel.Id;
            Name = jewel.Name;
            SlotSize = jewel.SlotSize;
            Abilities = jewel.Abilities;
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
