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
    public class DataSource : ISkillDataSource, IArmorDataSource
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

        public async Task<IArmorPiece[]> GetArmorPieces()
        {
            if (loadTask == null)
                loadTask = LoadData(null);

            await loadTask;

            return armors;
        }

        private Task loadTask;

        private IAbility[] abilities;
        private ISkill[] skills;
        private IArmorPiece[] armors;

        string ISkillDataSource.Description { get { return "http://mhw-db.com (skills API)"; } }
        string IArmorDataSource.Description { get { return "http://mhw-db.com (armor API)"; } }

        private async Task LoadData(ILogger logger)
        {
            await LoadSkillsData(logger);
            await LoadArmorsData(logger); // <- this must be called after LoadSkillsData
        }

        private async Task<IList<T>> LoadBase<T>(string api, ILogger logger)
        {
            string content;

            var dataAccess = new HttpDataAccess(
                logger,
                "mhwdb",
                TimeSpan.FromHours(24.0),
                httpClient => httpClient.GetStringAsync($"https://mhw-db.com/{api}")
            );

            for (int tryCount = 0; tryCount < 2; tryCount++)
            {
                content = await dataAccess.GetRawData(api);

                if (content == null)
                {
                    dataAccess.InvalidateCache(api);
                    continue;
                }

                try
                {
                    return JsonConvert.DeserializeObject<IList<T>>(content);
                }
                catch
                {
                    dataAccess.InvalidateCache(api);
                }
            }

            return null;
        }

        private async Task LoadArmorsData(ILogger logger)
        {
            IList<ArmorPiecePrimitive> result = await LoadBase<ArmorPiecePrimitive>("armor", logger);

            var allArmors = new IArmorPiece[result.Count];

            for (int i = 0; i < allArmors.Length; i++)
                allArmors[i] = new ArmorPiece(result[i], abilities);

            armors = allArmors;
        }

        private async Task LoadSkillsData(ILogger logger)
        {
            IList<SkillPrimitive> result = await LoadBase<SkillPrimitive>("skills", logger);

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
                        logger?.LogError($"Ability identifier '{ability.Id}' is a duplicate");
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
