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
    public class DataSource : ISkillDataSource, IArmorDataSource, ICharmDataSource, IJewelDataSource
    {
        private readonly ILogger logger;
        private readonly bool hasWriteAccess;

        public DataSource(ILogger logger, bool hasWriteAccess)
        {
            this.logger = logger;
            this.hasWriteAccess = hasWriteAccess;
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

        public async Task<ICharm[]> GetCharms()
        {
            if (loadTask == null)
                loadTask = LoadData(null);

            await loadTask;

            return charms;
        }

        public async Task<IJewel[]> GetJewels()
        {
            if (loadTask == null)
                loadTask = LoadData(null);

            await loadTask;

            return jewels;
        }

        private Task loadTask;

        private IAbility[] abilities;
        private ISkill[] skills;
        private IArmorPiece[] armors;
        private ICharm[] charms;
        private IJewel[] jewels;

        string ISkillDataSource.Description { get { return "http://mhw-db.com (skills API)"; } }
        string IArmorDataSource.Description { get { return "http://mhw-db.com (armor API)"; } }
        string ICharmDataSource.Description { get { return "http://mhw-db.com (charms API)"; } }
        string IJewelDataSource.Description { get { return "http://mhw-db.com (jewels API)"; } }

        private async Task LoadData(ILogger logger)
        {
            await LoadSkillsData(logger);

            await Task.WhenAll( // <- this must be called after LoadSkillsData
                LoadArmorsData(logger),
                LoadCharmsData(logger),
                LoadJewelsData(logger)
            );
        }

        private async Task<IList<T>> LoadBase<T>(string api, ILogger logger)
        {
            string content;

            var dataAccess = new HttpDataAccess(
                logger,
                hasWriteAccess,
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

            if (result == null)
                return;

            var allArmors = new IArmorPiece[result.Count];

            for (int i = 0; i < allArmors.Length; i++)
                allArmors[i] = new DataStructures.ArmorPiece(result[i], abilities);

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
                var skill = new DataStructures.Skill(skillPrimitive.Id, skillPrimitive.Name, skillPrimitive.Description);
                var localAbilities = new IAbility[skillPrimitive.Abilities.Count];
                int localAbilityCount = 0;

                foreach (AbilityPrimitive abilityPrimitive in skillPrimitive.Abilities)
                {
                    var ability = new Ability(skill, abilityPrimitive.Level, abilityPrimitive.Description);
                    if (allAbilities.Add(ability) == false)
                        logger?.LogError($"Ability identifier 'skill {ability.Skill.Name} level {ability.Level}' is a duplicate");
                    localAbilities[localAbilityCount++] = ability;
                }

                skill.SetAbilities(localAbilities);

                allSkills[localSkillCount++] = skill;
            }

            abilities = allAbilities.ToArray();
            skills = allSkills;
        }

        private async Task LoadCharmsData(ILogger logger)
        {
            IList<CharmPrimitive> result = await LoadBase<CharmPrimitive>("charms", logger);

            if (result == null)
                return;

            var localCharms = new ICharm[result.Count];

            for (int i = 0; i < localCharms.Length; i++)
            {
                CharmPrimitive currentCharmPrimitive = result[i];

                var charmLevels = new DataStructures.CharmLevel[currentCharmPrimitive.Levels.Length];

                for (int j = 0; j < charmLevels.Length; j++)
                    charmLevels[j] = new DataStructures.CharmLevel(i + 1, currentCharmPrimitive.Levels[j], abilities);

                localCharms[i] = new Charm(i + 1, currentCharmPrimitive.Name, charmLevels);
            }

            charms = localCharms;
        }

        private async Task LoadJewelsData(ILogger logger)
        {
            IList<JewelPrimitive> result = await LoadBase<JewelPrimitive>("decorations", logger);

            if (result == null)
                return;

            var localJewels = new DataStructures.Jewel[result.Count];

            for (int i = 0; i < localJewels.Length; i++)
                localJewels[i] = new DataStructures.Jewel(result[i], abilities);

            jewels = localJewels;
        }
    }
}
