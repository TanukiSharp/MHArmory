using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public enum VisibilityMode
    {
        All,
        Selected,
        Unselected
    }

    public class SkillSelectorViewModel : ViewModelBase
    {
        private IEnumerable<SkillViewModel> skills;
        public IEnumerable<SkillViewModel> Skills
        {
            get { return skills; }
            set { SetValue(ref skills, value); }
        }

        private VisibilityMode visibilityMode = VisibilityMode.All;

        public bool VisibilityModeAll
        {
            set
            {
                if (value && visibilityMode != VisibilityMode.All)
                {
                    visibilityMode = VisibilityMode.All;
                    ComputeVisibility();
                }
            }
        }

        public bool VisibilityModeSelected
        {
            set
            {
                if (value && visibilityMode != VisibilityMode.Selected)
                {
                    visibilityMode = VisibilityMode.Selected;
                    ComputeVisibility();
                }
            }
        }

        public bool VisibilityModeUnselected
        {
            set
            {
                if (value && visibilityMode != VisibilityMode.Unselected)
                {
                    visibilityMode = VisibilityMode.Unselected;
                    ComputeVisibility();
                }
            }
        }

        //private IEnumerable<AbilityViewModel> abilities;
        //public IEnumerable<AbilityViewModel> Abilities
        //{
        //    get { return abilities; }
        //    set { SetValue(ref abilities, value); }
        //}

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (SetValue(ref searchText, value))
                {
                    if (Skills != null)
                        ComputeVisibility();
                }
            }
        }

        private void ComputeVisibility()
        {
            foreach (SkillViewModel vm in Skills)
                ComputeVisibility(vm);
        }

        internal void ComputeVisibility(SkillViewModel skillViewModel)
        {
            if (visibilityMode == VisibilityMode.Selected)
            {
                if (skillViewModel.HasCheckedAbility == false)
                {
                    skillViewModel.IsVisible = false;
                    return;
                }
            }
            else if (visibilityMode == VisibilityMode.Unselected)
            {
                if (skillViewModel.HasCheckedAbility)
                {
                    skillViewModel.IsVisible = false;
                    return;
                }
            }

            skillViewModel.ApplySearchText(SearchStatement.Create(searchText));
        }

        public SkillSelectorViewModel()
        {
            //GlobalData.Instance.GetAbilities().ContinueWith(x => Abilities = x.Result);
        }
    }
}
