using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MHArmory.ArmoryDataSource.DataStructures;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MHArmory.ArmoryDataSource
{
    public class DataSource : IDataSource
    {
        public string Description { get; } = "Armory data source";

        private readonly ILogger logger;
        private readonly string dataPath;

        public DataSource(ILogger logger)
        {
            this.logger = logger;

            dataPath = Path.Combine(AppContext.BaseDirectory, "data");
        }

        public Task<IArmorPiece[]> GetArmorPieces()
        {
            throw new NotImplementedException();
        }

        public Task<ICharm[]> GetCharms()
        {
            throw new NotImplementedException();
        }

        private IJewel[] jewels;
        private Task<IJewel[]> jewelsTask;

        public Task<IJewel[]> GetJewels()
        {
            if (jewelsTask != null)
                return jewelsTask;

            GetSkills().Wait();

            string jewelsContent = File.ReadAllText(Path.Combine(dataPath, "jewels.json"));

            IList<JewelPrimitive> jewelPrimitives = JsonConvert.DeserializeObject<IList<JewelPrimitive>>(jewelsContent);

            jewels = jewelPrimitives
                .Select(x => new Jewel(
                    x.Id,
                    x.Name,
                    x.Rarity,
                    x.SlotSize,
                    x.AbilityIds.Join(abilities, a => a, b => b.Id, (a, b) => b).ToArray()
                ))
                .ToArray<IJewel>();

            jewelsTask = Task.FromResult(jewels);

            return jewelsTask;
        }

        private ISkill[] skills;
        private IList<IAbility> abilities;
        private Task<ISkill[]> skillsTask;

        public Task<ISkill[]> GetSkills()
        {
            if (skillsTask != null)
                return skillsTask;

            string abilitiesContent = File.ReadAllText(Path.Combine(dataPath, "abilities.json"));
            string skillsContent = File.ReadAllText(Path.Combine(dataPath, "skills.json"));

            IList<SkillPrimitive> skillPrimitives = JsonConvert.DeserializeObject<IList<SkillPrimitive>>(skillsContent);
            IList<AbilityPrimitive> abilityPrimitives = JsonConvert.DeserializeObject<IList<AbilityPrimitive>>(abilitiesContent);

            abilities = abilityPrimitives
                .Select(x => new DataStructures.Ability(x))
                .ToList<IAbility>();

            skills = skillPrimitives
                .Select(x => new Skill(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.AbilityIds.Join(abilities, a => a, b => b.Id, (a, b) => b).ToArray<IAbility>()))
                .ToArray<ISkill>();

            foreach (ISkill skill in skills)
            {
                foreach (DataStructures.Ability ability in skill.Abilities)
                    ability.Update(skill);
            }

            skillsTask = Task.FromResult(skills);

            return skillsTask;
        }
    }
}
