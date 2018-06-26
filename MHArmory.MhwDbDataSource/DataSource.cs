using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.MhwDbDataSource.DataStructures;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace MHArmory.MhwDbDataSource
{
    public class DataSource : ISkillDataSource
    {
        private readonly ILogger logger;

        public DataSource(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<IAbility[]> GetAbilities()
        {
            if (loadTask == null)
                loadTask = LoadData(null);

            await loadTask;

            return abilities;
        }

        public async Task<ISkill[]> GetSkills()
        {
            if (loadTask == null)
                loadTask = LoadData(null);

            await loadTask;

            return skills;
        }

        private Task loadTask;

        private IAbility[] abilities;
        private ISkill[] skills;

        private async Task LoadData(ILogger logger)
        {
            string content;
            IList<SkillPrimitive> result = null;

            const string api = "skills";

            var dataAccess = new HttpDataAccess(
                logger,
                "mhwdb",
                TimeSpan.FromHours(24.0),
                httpClient => httpClient.GetStringAsync($"https://mhw-db.com/{api}")
            );

            for (int tryCount = 0; tryCount < 2; tryCount++)
            {
                content = await dataAccess.GetRawData(api);

                try
                {
                    result = JsonConvert.DeserializeObject<IList<SkillPrimitive>>(content);
                    break;
                }
                catch
                {
                    dataAccess.InvalidateCache(api);
                }
            }

            if (result == null)
                return;

            var allAbilities = new HashSet<IAbility>();
            var allSkills = new ISkill[result.Count];
            int localSkillCount = 0;

            foreach (SkillPrimitive skillPrimitive in result)
            {
                var skill = new Skill(skillPrimitive.Id, skillPrimitive.Name, skillPrimitive.Description);
                var localAbilities = new IAbility[skillPrimitive.Abilities.Count];
                int localAbilityCount = 0;

                foreach (AbilityPrimitive abilityPrimitive in skillPrimitive.Abilities)
                {
                    var ability = new Ability(abilityPrimitive.Id, skill, abilityPrimitive.Level, abilityPrimitive.Description);
                    if (allAbilities.Add(ability) == false)
                        throw new FormatException($"Ability identifier '{ability.Id}' is a duplicate");
                    localAbilities[localAbilityCount++] = ability;
                }

                skill.SetAbilities(localAbilities);

                allSkills[localSkillCount++] = skill;
            }

            abilities = allAbilities.ToArray();
            skills = allSkills;
        }
    }
}
