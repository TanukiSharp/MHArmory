using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MHArmory.ArmoryDataSource.DataStructures;
using MHArmory.Core;
using Newtonsoft.Json;

namespace DataSourceTool
{
    public class MhwdbSkillPrimitive
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class ExporterSkills
    {
        public async Task Run(string[] args)
        {
            string solutionPath = Common.FindSolutionPath();
            if (solutionPath == null)
                throw new InvalidOperationException($"Could not find solution path for '{Common.SolutionFilename}'");

            string outputPath = Path.Combine(solutionPath, "MHArmory", "data");

            if (Directory.Exists(outputPath) == false)
                Directory.CreateDirectory(outputPath);

            string targetFilename = Path.Combine(outputPath, "skills.json");

            string defaultContent = File.ReadAllText(targetFilename);
            SkillPrimitive[] defaultSkillPrimitives = JsonConvert.DeserializeObject<SkillPrimitive[]>(defaultContent);

            var httpClient = new HttpClient();
            string mhwdbContent = await httpClient.GetStringAsync("http://mhw-db.com/skills");

            MhwdbSkillPrimitive[] mhwdbSkillPrimitives = JsonConvert.DeserializeObject<MhwdbSkillPrimitive[]>(mhwdbContent);

            foreach (MhwdbSkillPrimitive x in mhwdbSkillPrimitives)
            {
                SkillPrimitive defaultSkillPrimitive = defaultSkillPrimitives.FirstOrDefault(y => Localization.GetDefault(y.Name) == x.Name);
                if (defaultSkillPrimitive == null)
                {
                    Console.WriteLine($"Skill '{x.Name}' found in mhw-db.com is missing from Armory data source.");
                    continue;
                }

                defaultSkillPrimitive.Description[Localization.DefaultLanguage] = x.Description;
            }

            foreach (SkillPrimitive x in defaultSkillPrimitives)
            {
                MhwdbSkillPrimitive mhwdbSkillPrimitive = mhwdbSkillPrimitives.FirstOrDefault(y => y.Name == Localization.GetDefault(x.Name));
                if (mhwdbSkillPrimitive == null)
                    Console.WriteLine($"Skill '{x.Name}' found in Armory data source is missing from mhw-db.com.");
            }

            Common.SerializeJson(targetFilename, defaultSkillPrimitives);
        }
    }
}
