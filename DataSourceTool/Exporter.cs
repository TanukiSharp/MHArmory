using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MHArmory.ArmoryDataSource.DataStructures;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DataSourceTool
{
    public class Exporter
    {
        public async Task Run(string[] args)
        {
            ILogger logger = new ConsoleLogger();

            //var httpClient = new HttpClient();
            //Task<string> fetchWeaponsTask = httpClient.GetStringAsync("https://mhw-db.com/weapons?p={\"slug\":false,\"crafting\":false,\"assets\":false}");

            IDataSource source = new MHArmory.AthenaAssDataSource.DataSource(logger, null, null);

            IList<IArmorPiece> armorPieces = (await source.GetArmorPieces()).OrderBy(x => x.Id).ToList();
            IList<ISkill> skills = (await source.GetSkills()).OrderBy(x => x.Id).ToList();
            IList<ICharm> charms = (await source.GetCharms()).OrderBy(x => x.Id).ToList();
            IList<IJewel> jewels = (await source.GetJewels()).OrderBy(x => x.Id).ToList();

            IList<IAbility> abilities = skills
                .SelectMany(x => x.Abilities)
                .Distinct(AbilityEqualityComparer.Default)
                .OrderBy(x => x.Id)
                .ToList();

            string solutionPath = Common.FindSolutionPath();
            if (solutionPath == null)
                throw new InvalidOperationException($"Could not find solution path for '{Common.SolutionFilename}'");

            string outputPath = Path.Combine(solutionPath, "MHArmory", "data");

            if (Directory.Exists(outputPath) == false)
                Directory.CreateDirectory(outputPath);

            //--------------------------------------------------------------------------

            Common.SerializeJson(Path.Combine(outputPath, $"{nameof(abilities)}.json"), Export(abilities));

            //--------------------------------------------------------------------------

            IEnumerable<IArmorPiece> heads = armorPieces.Where(x => x.Type == EquipmentType.Head);
            IEnumerable<IArmorPiece> chests = armorPieces.Where(x => x.Type == EquipmentType.Chest);
            IEnumerable<IArmorPiece> arms = armorPieces.Where(x => x.Type == EquipmentType.Gloves);
            IEnumerable<IArmorPiece> waists = armorPieces.Where(x => x.Type == EquipmentType.Waist);
            IEnumerable<IArmorPiece> legs = armorPieces.Where(x => x.Type == EquipmentType.Legs);

            Common.SerializeJson(Path.Combine(outputPath, $"{nameof(heads)}.json"), Export(heads));
            Common.SerializeJson(Path.Combine(outputPath, $"{nameof(chests)}.json"), Export(chests));
            Common.SerializeJson(Path.Combine(outputPath, $"{nameof(arms)}.json"), Export(arms));
            Common.SerializeJson(Path.Combine(outputPath, $"{nameof(waists)}.json"), Export(waists));
            Common.SerializeJson(Path.Combine(outputPath, $"{nameof(legs)}.json"), Export(legs));

            //--------------------------------------------------------------------------

            IList<IArmorSetSkill> armorSetSkills = armorPieces
                .Where(x => x.ArmorSetSkills != null && x.ArmorSetSkills.Length > 0)
                .SelectMany(x => x.ArmorSetSkills)
                .Distinct(new LambdaEqualityComparer<IArmorSetSkill>((x, y) => x.Id == y.Id, x => x.Id))
                .OrderBy(x => x.Id)
                .ToList();
            Common.SerializeJson(Path.Combine(outputPath, $"{nameof(armorSetSkills)}.json"), Export(armorSetSkills));

            //--------------------------------------------------------------------------

            IList<IFullArmorSet> fullArmorSets = armorPieces
                .Select(x => x.FullArmorSet)
                .Where(x => x != null)
                .Distinct(new LambdaEqualityComparer<IFullArmorSet>((x, y) => x.Id == y.Id, x => x.Id))
                .OrderBy(x => x.Id)
                .ToList();
            Common.SerializeJson(Path.Combine(outputPath, $"{nameof(fullArmorSets)}.json"), Export(fullArmorSets));

            //--------------------------------------------------------------------------

            IList<ICharmLevel> charmLevels = charms
                .SelectMany(x => x.Levels)
                .Distinct(new LambdaEqualityComparer<ICharmLevel>((x, y) => x.Id == y.Id, x => x.Id))
                .OrderBy(x => x.Id)
                .ToList();
            Common.SerializeJson(Path.Combine(outputPath, $"{nameof(charmLevels)}.json"), Export(charmLevels));

            //--------------------------------------------------------------------------

            Common.SerializeJson(Path.Combine(outputPath, $"{nameof(charms)}.json"), Export(charms));

            //--------------------------------------------------------------------------

            Common.SerializeJson(Path.Combine(outputPath, $"{nameof(skills)}.json"), Export(skills));

            //--------------------------------------------------------------------------

            Common.SerializeJson(Path.Combine(outputPath, $"{nameof(jewels)}.json"), Export(jewels));
        }

        private object Export(IEnumerable<IAbility> abilities)
        {
            return abilities.Select((x, i) => new AbilityPrimitive
            {
                Id = i,
                Level = x.Level,
                Description = x.Description
            });
        }

        private object Export(IEnumerable<IArmorPiece> armorPieces)
        {
            return armorPieces.Select(x => new ArmorPiecePrimitive
            {
                Id = x.Id,
                Name = x.Name,
                Rarity = x.Rarity,
                Slots = MakeSlots(x.Slots),
                AbilityIds = MakeAbilities(x.Abilities),
                Attributes = new ArmorAttributesPrimitive
                {
                    Gender = x.Attributes.RequiredGender
                },
                Defense = new ArmorDefensePrimitive
                {
                    Base = x.Defense.Base,
                    Max = x.Defense.Max,
                    Augmented = x.Defense.Augmented
                },
                Resistances = new ArmorResistancesPrimitive
                {
                    Fire = x.Resistances.Fire,
                    Water = x.Resistances.Water,
                    Thunder = x.Resistances.Thunder,
                    Ice = x.Resistances.Ice,
                    Dragon = x.Resistances.Dragon
                },
                ArmorSetSkillIds = x.ArmorSetSkills?.Select(s => s.Id).ToList(),
                FullArmorSetId = x.FullArmorSet?.Id,
                EventId = x.Event?.Id
            });
        }

        private object Export(IEnumerable<IArmorSetSkill> armorSetSkills)
        {
            return armorSetSkills.Select(x => new ArmorSetSkillPrimitive
            {
                Id = x.Id,
                Name = x.Name,
                Parts = x.Parts.Select(p => new ArmorSetSkillPartPrimitive
                {
                    Id = p.Id,
                    RequiredArmorPieceCount = p.RequiredArmorPieces,
                    GrantedSkills = p.GrantedSkills.Select(a => a.Id).ToList()
                }).ToList(),
            });
        }

        private object Export(IList<IFullArmorSet> fullArmorSets)
        {
            return fullArmorSets.Select(x => new FullArmorSetPrimitive
            {
                Id = x.Id,
                ArmorPieceIds = x.ArmorPieces.Select(p => p.Id).ToList()
            });
        }

        private object Export(IEnumerable<ICharmLevel> charmLevels)
        {
            return charmLevels.Select(x => new CharmLevelPrimitive
            {
                Id = x.Id,
                Name = x.Name,
                Level = x.Level,
                Rarity = x.Rarity,
                AbilityIds = MakeAbilities(x.Abilities),
                Slots = MakeSlots(x.Slots),
                EventId = x.Event?.Id
            });
        }

        private object Export(IEnumerable<ICharm> charms)
        {
            return charms.Select(x => new CharmPrimitive
            {
                Id = x.Id,
                Name = x.Name,
                LevelIds = x.Levels.Select(c => c.Id).ToList()
            });
        }

        private object Export(IEnumerable<IEvent> events)
        {
            return events.Select(x => new EventPrimitive
            {
                Id = x.Id,
                Name = null
            });
        }

        private object Export(IEnumerable<ISkill> skills)
        {
            return skills.Select(x => new SkillPrimitive
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                AbilityIds = MakeAbilities(x.Abilities),
                Categories = x.Categories
            });
        }

        private object Export(IList<IJewel> jewels)
        {
            return jewels.Select(x => new JewelPrimitive
            {
                Id = x.Id,
                Name = x.Name,
                Rarity = x.Rarity,
                SlotSize = x.SlotSize,
                AbilityIds = MakeAbilities(x.Abilities)
            });
        }

        //=============================================================================================

        private int[] MakeSlots(int[] slots)
        {
            if (slots != null && slots.Length > 0 && slots.Any(x => x > 0))
                return slots.OrderByDescending(x => x).ToArray();

            return null;
        }

        private IList<int> MakeAbilities(IAbility[] abilities)
        {
            if (abilities != null && abilities.Length > 0)
                return abilities.Select(a => a.Id).ToList();

            return null;
        }
    }
}
