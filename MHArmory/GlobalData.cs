using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.ViewModels;

namespace MHArmory
{
    public class GlobalData
    {
        public static readonly GlobalData Instance = new GlobalData();

        private readonly TaskCompletionSource<AbilityViewModel[]> abilitiesTaskCompletionSource = new TaskCompletionSource<AbilityViewModel[]>();

        public void SetAbilities(AbilityViewModel[] abilities)
        {
            abilitiesTaskCompletionSource.TrySetResult(abilities);
        }

        public Task<AbilityViewModel[]> GetAbilities()
        {
            return abilitiesTaskCompletionSource.Task;
        }

        private readonly TaskCompletionSource<SkillViewModel[]> skillsTaskCompletionSource = new TaskCompletionSource<SkillViewModel[]>();

        public void SetSkills(SkillViewModel[] skills)
        {
            skillsTaskCompletionSource.TrySetResult(skills);
        }

        public Task<SkillViewModel[]> GetSkills()
        {
            return skillsTaskCompletionSource.Task;
        }
    }
}
