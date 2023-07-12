using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using md = MHWMasterDataUtils.Core;
using mha_ds = MHArmory.ArmoryDataSource.DataStructures;
using mha_coreds = MHArmory.Core.DataStructures;
using System.Linq;

namespace DataSourceTool
{
    public static class SkillImporter
    {
        public static async Task<mha_coreds.ISkill[]> Run(MasterDataDownloader downloader)
        {
            string skillsContent = await downloader.DownloadFile("skills.json");

            md.Skill[] inputSkills = JsonConvert.DeserializeObject<md.Skill[]>(skillsContent);

            foreach (md.Skill skill in inputSkills)
            {
                foreach (md.Ability ability in skill.Abilities)
                {
                    if (ability.SkillId == null)
                        ability.SkillId = skill.Id;
                }
            }

            //mha_ds.Ability[] abilities = inputSkills
            //    .SelectMany(x => x.Abilities)
            //    .Where(x => x.Level.HasValue)
            //    .Select((a, i) =>
            //    {
            //        return new mha_ds.Ability(new mha_ds.AbilityPrimitive
            //        {
            //            Id = a.SkillId.Value,
            //            Description = a.Description,
            //            Level = a.Level.Value
            //        });
            //    })
            //    .ToArray();

            var skills = new List<mha_coreds.ISkill>();

            foreach (md.Skill skill in inputSkills)
            {
                //mha_coreds.IAbility[] skillAbilities = abilities.Where(x => x.Id == skill.Id).ToArray();


                var skillAbilities = new List<mha_coreds.IAbility>();

                foreach (md.Ability ability in skill.Abilities)
                {
                    skillAbilities.Add(new mha_ds.Ability(new mha_ds.AbilityPrimitive
                    {
                        Id = (int)ability.Id
                    }));
                }

                skills.Add(new mha_coreds.Skill(skill.Id, skill.Name, skill.Description, skillAbilities.ToArray(), Array.Empty<string>()));
            }

            return skills.ToArray();
        }
    }
}
