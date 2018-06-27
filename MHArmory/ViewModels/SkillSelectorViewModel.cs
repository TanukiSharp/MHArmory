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
    public class SkillSelectorViewModel : ViewModelBase
    {
        private IEnumerable<SkillViewModel> skills;
        public IEnumerable<SkillViewModel> Skills
        {
            get { return skills; }
            set { SetValue(ref skills, value); }
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
                    {
                        foreach (SkillViewModel vm in Skills)
                            vm.ApplySearchText(searchText);
                    }
                }
            }
        }

        public SkillSelectorViewModel()
        {
            //GlobalData.Instance.GetAbilities().ContinueWith(x => Abilities = x.Result);
            GlobalData.Instance.GetSkills().ContinueWith(x => Skills = x.Result);
        }
    }
}
