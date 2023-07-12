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
    public static class AbilitiesImporter
    {
        public static async Task<mha_coreds.IAbility[]> Run(MasterDataDownloader downloader)
        {
            string skillsContent = await downloader.DownloadFile("skills.json");

            md.Skill[] inputSkills = JsonConvert.DeserializeObject<md.Skill[]>(skillsContent);

            mha_ds.Ability[] abilities = inputSkills
                .SelectMany(x => x.Abilities)
                .Select(a =>
                {
                    return new mha_ds.Ability(new mha_ds.AbilityPrimitive
                    {
                        Id = (int)a.Id,
                        Name = a.Name,
                        Description = a.Description,
                        Level = a.Level ?? 0
                    });
                })
                .ToArray();

            return abilities;
        }
    }
}
