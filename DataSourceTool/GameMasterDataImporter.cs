using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using md = MHWMasterDataUtils.Core;
using mha_ds = MHArmory.ArmoryDataSource.DataStructures;
using mha_coreds = MHArmory.Core.DataStructures;
using System.Linq;
using System.Reflection;

namespace DataSourceTool
{


    public class GameMasterDataImporter
    {
        private readonly MasterDataDownloader downloader = new MasterDataDownloader();

        private static string CharmToGroupName(md.Charm charm)
        {
            if (charm.Skills == null || charm.Skills.Length == 0)
            {
                return null;
            }

            return string.Join(":", charm.Skills.OrderBy(x => x.SkillId).Select(x => x.SkillId));
        }

        private static string FindCommonName(md.Charm[] charms)
        {
            string baseName = charms[0].Name[md.LanguageUtils.DefaultLanguageCode];

            if (charms.Length == 1)
                return baseName;

            string otherName = charms[1].Name[md.LanguageUtils.DefaultLanguageCode];

            int length = Math.Min(baseName.Length, otherName.Length);

            for (int i = 0; i < length; i++)
            {
                if (baseName[i] != otherName[i])
                    return baseName.Substring(0, i - 1).Trim();
            }

            return null;
        }

        private static MHArmory.Core.DataStructures.ICharm CharmGroupToCharm(int index, IGrouping<string, md.Charm> group)
        {
            string commonName = FindCommonName(group.ToArray());
            return new MHArmory.Core.DataStructures.Charm(index, null, Array.Empty<MHArmory.Core.DataStructures.ICharmLevel>());
        }

        //public async Task Run()
        //{
        //    string skillsContent = await downloader.DownloadFile("skills.json");

        //    md.Skill[] inputSkills = JsonConvert.DeserializeObject<md.Skill[]>(skillsContent);

        //    mha_ds.Ability[] abilities = inputSkills
        //        .SelectMany(x => x.Abilities)
        //        .Select((a, i) => new mha_ds.Ability(new mha_ds.AbilityPrimitive
        //        {
        //            Id = a.SkillId ?? throw new Exception("Must have a skillId"),
        //            Description = a.Description,
        //            Level = a.Level ?? throw new Exception("Must have a level")
        //        }))
        //        .ToArray();

        //    var skills = new List<mha_coreds.ISkill>();

        //    foreach (md.Skill skill in inputSkills)
        //    {
        //        mha_coreds.IAbility[] skillAbilities = abilities.Where(x => x.Id == skill.Id).ToArray<mha_coreds.IAbility>();

        //        skills.Add(new mha_coreds.Skill(skill.Id, skill.Name, skill.Description, skillAbilities, Array.Empty<string>()));
        //    }

        //    Common.SerializeJson(Path.Combine(outputPath, $"{nameof(skills)}.json"), Export(skills))

        //    string charmsContent = await downloader.DownloadFile("charms.json");

        //    md.Charm[] inputCharms = JsonConvert.DeserializeObject<md.Charm[]>(charmsContent);

        //    //int id = 1;
        //    foreach (IGrouping<string, md.Charm> charmGroup in inputCharms.GroupBy(CharmToGroupName))
        //    {
        //        int count = charmGroup.Count();
        //        string name = charmGroup.ElementAt(0).Name["eng"];

        //        if (charmGroup.Any(x => x.Skills == null))
        //        {
        //        }

        //        if (count == 1 && charmGroup.Any(x => x.Skills != null && x.Skills.Length > 1))
        //        {
        //        }

        //        int level = 1;
        //        foreach (md.Charm charm in charmGroup)
        //        {
        //            new mha.CharmLevel(charm.Id, level++, charm.Name, charm.Rarity, Array.Empty<int>(), charm.Skills.Select(s => new mha.Ability(s.SkillId
        //        }

        //        new mha.Charm(id++, charmGroup.ElementAt(0).Name, ??);
        //    }

        //    //    .Select((x, i) => new Charm()
        //    //    .ToArray();

        //    //    .SelectMany(x => x.Skills)
        //    //    .Distinct(new LambdaEqualityComparer<ICharmLevel>((x, y) => x.Id == y.Id, x => x.Id))
        //    //    .OrderBy(x => x.Id)
        //    //    .ToList();
        //    //Common.SerializeJson(Path.Combine(outputPath, $"{nameof(charmLevels)}.json"), Export(charmLevels));
        //}
    }
}
