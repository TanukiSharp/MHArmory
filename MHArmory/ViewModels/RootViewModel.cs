using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core;

namespace MHArmory.ViewModels
{
    public class RootViewModel : ViewModelBase
    {
        public SkillSelectorViewModel SkillSelector { get; }

        public RootViewModel()
        {
            SkillSelector = new SkillSelectorViewModel();
        }
    }
}
