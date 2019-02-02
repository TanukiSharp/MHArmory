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

        private IList<IArmorSetSkill> armorSetSkills;

        private void FetchArmorSetSkills()
        {
            if (armorSetSkills != null)
                return;

            string armorSetSkillsContent = File.ReadAllText(Path.Combine(dataPath, "armorSetSkills.json"));
            IList<ArmorSetSkillPrimitive> armorSetSkillPrimitives = JsonConvert.DeserializeObject<IList<ArmorSetSkillPrimitive>>(armorSetSkillsContent);

            armorSetSkills = armorSetSkillPrimitives.Select(x => new ArmorSetSkill(
                x.Id,
                x.Name,
                x.Parts.Select(p => new ArmorSetSkillPart(
                    p.Id,
                    p.RequiredArmorPieceCount,
                    p.GrantedSkills.JoinEx(abilities, inner => inner.Id).ToArray()
                ))
                .ToArray()
            ))
            .ToArray();
        }

        private IList<IFullArmorSet> fullArmorSets;

        private void FetchFullArmorSets(IList<(ArmorPiece armorPiece, int? fullArmorSetId)> armorPieces)
        {
            if (fullArmorSets != null)
                return;

            IArmorPiece[] GetFullArmorPieces(IList<int> indices)
            {
                if (indices.Count != 5)
                    throw new InvalidDataSourceException($"Invalid armor pieces count: {indices.Count}, expected 5.");

                return new IArmorPiece[]
                {
                    armorPieces.FirstOrDefault(x => x.armorPiece.Id == indices[0] && x.armorPiece.Type == EquipmentType.Head).armorPiece,
                    armorPieces.FirstOrDefault(x => x.armorPiece.Id == indices[1] && x.armorPiece.Type == EquipmentType.Chest).armorPiece,
                    armorPieces.FirstOrDefault(x => x.armorPiece.Id == indices[2] && x.armorPiece.Type == EquipmentType.Gloves).armorPiece,
                    armorPieces.FirstOrDefault(x => x.armorPiece.Id == indices[3] && x.armorPiece.Type == EquipmentType.Waist).armorPiece,
                    armorPieces.FirstOrDefault(x => x.armorPiece.Id == indices[4] && x.armorPiece.Type == EquipmentType.Legs).armorPiece
                };
            }

            string fullArmorSetsContent = File.ReadAllText(Path.Combine(dataPath, "fullArmorSets.json"));
            IList<FullArmorSetPrimitive> fullArmorSetPrimitives = JsonConvert.DeserializeObject<IList<FullArmorSetPrimitive>>(fullArmorSetsContent);

            fullArmorSets = fullArmorSetPrimitives.Select(x => new FullArmorSet(
                x.Id,
                GetFullArmorPieces(x.ArmorPieceIds)
            ))
            .ToList<IFullArmorSet>();
        }

        private IFullArmorSet FindFullArmorSetById(int id)
        {
            foreach (IFullArmorSet fullArmorSet in fullArmorSets)
            {
                if (fullArmorSet.Id == id)
                    return fullArmorSet;
            }

            logger?.LogError($"Impossible to find full armor set with id {id}.");

            return null;
        }

        private Task<IArmorPiece[]> armorPiecesTask;

        public Task<IArmorPiece[]> GetArmorPieces()
        {
            if (armorPiecesTask != null)
                return armorPiecesTask;

            FetchSkills();
            FetchArmorSetSkills();

            string headsContent = File.ReadAllText(Path.Combine(dataPath, "heads.json"));
            string chestsContent = File.ReadAllText(Path.Combine(dataPath, "chests.json"));
            string armsContent = File.ReadAllText(Path.Combine(dataPath, "arms.json"));
            string waistsContent = File.ReadAllText(Path.Combine(dataPath, "waists.json"));
            string legsContent = File.ReadAllText(Path.Combine(dataPath, "legs.json"));

            IList<ArmorPiecePrimitive> headPrimitives = JsonConvert.DeserializeObject<IList<ArmorPiecePrimitive>>(headsContent);
            IList<ArmorPiecePrimitive> chestPrimitives = JsonConvert.DeserializeObject<IList<ArmorPiecePrimitive>>(chestsContent);
            IList<ArmorPiecePrimitive> armPrimitives = JsonConvert.DeserializeObject<IList<ArmorPiecePrimitive>>(armsContent);
            IList<ArmorPiecePrimitive> waistPrimitives = JsonConvert.DeserializeObject<IList<ArmorPiecePrimitive>>(waistsContent);
            IList<ArmorPiecePrimitive> legPrimitives = JsonConvert.DeserializeObject<IList<ArmorPiecePrimitive>>(legsContent);

            var outputArmorPieces = new List<(ArmorPiece armorPiece, int? fullArmorSetId)>();

            LoadArmorPieces(EquipmentType.Head, headPrimitives, outputArmorPieces);
            LoadArmorPieces(EquipmentType.Chest, chestPrimitives, outputArmorPieces);
            LoadArmorPieces(EquipmentType.Gloves, armPrimitives, outputArmorPieces);
            LoadArmorPieces(EquipmentType.Waist, waistPrimitives, outputArmorPieces);
            LoadArmorPieces(EquipmentType.Legs, legPrimitives, outputArmorPieces);

            FetchFullArmorSets(outputArmorPieces);

            foreach ((ArmorPiece armorPiece, int? fullArmorSetId) in outputArmorPieces)
            {
                if (fullArmorSetId.HasValue)
                    armorPiece.SetFullArmorSet(FindFullArmorSetById(fullArmorSetId.Value));
            }

            armorPiecesTask = Task.FromResult(
                outputArmorPieces.Select(x => x.armorPiece).ToArray<IArmorPiece>()
            );

            return armorPiecesTask;
        }

        private static ArmorPieceResistances CreateResistances(ArmorResistancesPrimitive primitive)
        {
            return new ArmorPieceResistances(
                primitive.Fire,
                primitive.Water,
                primitive.Thunder,
                primitive.Ice,
                primitive.Dragon
            );
        }

        private void LoadArmorPieces(EquipmentType type, IList<ArmorPiecePrimitive> armorPiecePrimitives, List<(ArmorPiece armorPiece, int? fullArmorSetId)> output)
        {
            foreach (ArmorPiecePrimitive x in armorPiecePrimitives)
            {
                var armorPiece = new ArmorPiece(
                    x.Id,
                    x.Name,
                    type,
                    x.Rarity,
                    x.Slots?.ToArray() ?? Array.Empty<int>(),
                    x.AbilityIds?.JoinEx(abilities, inner => inner.Id).ToArray() ?? Array.Empty<IAbility>(),
                    x.ArmorSetSkillIds?.JoinEx(armorSetSkills, inner => inner.Id).ToArray(),
                    new ArmorPieceDefense(x.Defense.Base, x.Defense.Max, x.Defense.Augmented),
                    CreateResistances(x.Resistances),
                    new ArmorPieceAttributes(x.Attributes.Gender),
                    null,
                    null,
                    null
                );
                output.Add((armorPiece, x.FullArmorSetId));
            }
        }

        private ICharm[] charms;
        private Task<ICharm[]> charmsTask;

        public Task<ICharm[]> GetCharms()
        {
            if (charmsTask != null)
                return charmsTask;

            string charmsContent = File.ReadAllText(Path.Combine(dataPath, "charms.json"));
            string charmLevelsContent = File.ReadAllText(Path.Combine(dataPath, "charmLevels.json"));

            IList<CharmPrimitive> charmPrimitives = JsonConvert.DeserializeObject<IList<CharmPrimitive>>(charmsContent);
            IList<CharmLevelPrimitive> charmLevelPrimitives = JsonConvert.DeserializeObject<IList<CharmLevelPrimitive>>(charmLevelsContent);

            IList<ICharmLevel> charmLevels = charmLevelPrimitives.Select(x => new CharmLevel(
                x.Id,
                x.Level,
                x.Name,
                x.Rarity,
                x.Slots?.ToArray() ?? Array.Empty<int>(),
                x.AbilityIds?.JoinEx(abilities, inner => inner.Id).ToArray() ?? Array.Empty<IAbility>(),
                null
            ))
            .ToList<ICharmLevel>();

            charms = charmPrimitives.Select(x => new Charm(
                x.Id,
                x.Name,
                x.LevelIds.JoinEx(charmLevels, inner => inner.Id).ToArray()
            ))
            .ToArray<ICharm>();

            charmsTask = Task.FromResult(charms);

            return charmsTask;
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
                    x.AbilityIds?.JoinEx(abilities, inner => inner.Id).ToArray() ?? Array.Empty<IAbility>()
                ))
                .ToArray<IJewel>();

            jewelsTask = Task.FromResult(jewels);

            return jewelsTask;
        }

        private ISkill[] skills;
        private IList<IAbility> abilities;
        private Task<ISkill[]> skillsTask;

        private void FetchSkills()
        {
            if (skills != null)
                return;

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
                    x.AbilityIds?.JoinEx(abilities, inner => inner.Id).ToArray() ?? Array.Empty<IAbility>(),
                    x.Categories
                ))
                .ToArray<ISkill>();

            foreach (ISkill skill in skills)
            {
                foreach (DataStructures.Ability ability in skill.Abilities)
                    ability.Update(skill);
            }
        }

        public Task<ISkill[]> GetSkills()
        {
            if (skillsTask != null)
                return skillsTask;

            FetchSkills();

            skillsTask = Task.FromResult(skills);

            return skillsTask;
        }
    }

    public static class Operators
    {
        public static IEnumerable<TInner> JoinEx<TInner>(this IEnumerable<int> outer, IEnumerable<TInner> inner, Func<TInner, int> innerKeySelector)
        {
            return outer.Join(inner, a => a, innerKeySelector, (a, b) => b);
        }

        public static IEnumerable<TResult> JoinEx<TInner, TResult>(this IEnumerable<int> outer, IEnumerable<TInner> inner, Func<TInner, int> innerKeySelector, Func<TInner, TResult> resultSelector)
        {
            return outer.Join(inner, a => a, innerKeySelector, (a, b) => resultSelector(b));
        }
    }
}
