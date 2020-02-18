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
    public class DataSource : IDataSource
    {
        private readonly ILogger logger;
        private readonly bool hasWriteAccess;
        private readonly object loadLock = new object();
        private readonly Dictionary<string, string> localizationKeys = new Dictionary<string, string>() { { "fr", "FR" }, { "de", "DE" }, { "zh", "CN" } };
        private readonly string DB_KEY_SKILLS = "skills";
        private readonly string DB_KEY_DECORATIONS = "decorations";
        private readonly string DB_KEY_ARMORS = "armor";
        private readonly string DB_KEY_ARMOR_SETS = "armor/sets";
        private readonly string DB_KEY_CHARMS = "charms";
        private readonly string DB_KEY_ITEMS = "items";

        private Task loadTask;
        private IAbility[] abilities;
        private ISkill[] skills;
        private IArmorPiece[] armors;
        private ICharm[] charms;
        private IJewel[] jewels;
        private ILocalizedItem[] localizedItems;

        public string Description { get; } = "MHW-DB";

        public DataSource(ILogger logger, bool hasWriteAccess)
        {
            this.logger = logger;
            this.hasWriteAccess = hasWriteAccess;
        }

        private async Task load()
        {
            lock (loadLock)
            {
                if (loadTask == null)
                    loadTask = LoadData();
            }
            await loadTask;
        }

        public async Task<IAbility[]> GetAbilities()
        {
            await load();
            return abilities;
        }

        public async Task<ISkill[]> GetSkills()
        {
            await load();
            return skills;
        }

        public async Task<IArmorPiece[]> GetArmorPieces()
        {
            await load();
            return armors;
        }

        public async Task<ICharm[]> GetCharms()
        {
            await load();
            return charms;
        }

        public async Task<IJewel[]> GetJewels()
        {
            await load();
            return jewels;
        }

        public async Task<ILocalizedItem[]> GetCraftMaterials()
        {
            await load();
            return localizedItems;
        }


        private async Task LoadData()
        {
            await Task.WhenAll(
                LoadSkillsData(),
                LoadItemData()
            );

            await Task.WhenAll( // <- this must be called after LoadSkillsData and LoadItemData
                LoadArmorsData(),
                LoadCharmsData(),
                LoadJewelsData()
            );
        }

        private async Task<IList<T>> LoadBase<T>(string api)
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

        private async Task LoadItemData()
        {
            Task<IList<ItemPrimitive>> itemTask = LoadBase<ItemPrimitive>(DB_KEY_ITEMS);
            var itemLocalizations = new Dictionary<string, Task<IList<ItemPrimitive>>>();
            foreach (KeyValuePair<string, string> localization in localizationKeys)
            {
                itemLocalizations[localization.Value] = LoadBase<ItemPrimitive>(localization.Key + "/" + DB_KEY_ITEMS);
            }

            IList<ItemPrimitive> itemResult = await itemTask;

            if (itemResult == null)
            {
                logger?.LogError("Query for all items failed");
                return;
            }

            var allItems = new Dictionary<int, DataStructures.LocalizedItem>();
            foreach(ItemPrimitive item in itemResult)
            {
                allItems[item.Id] = new DataStructures.LocalizedItem(item);
            }

            foreach (KeyValuePair<string, Task<IList<ItemPrimitive>>> localizationQuery in itemLocalizations)
            {
                IList<ItemPrimitive> localization = await localizationQuery.Value;
                if (localization == null)
                {
                    logger?.LogError($"Query for item localization '{localizationQuery.Key}' failed");
                    continue;
                }
                foreach (ItemPrimitive item in localization)
                {
                    allItems[item.Id].AddLocalization(localizationQuery.Key, item);
                }
            }

            localizedItems = allItems.Values.ToArray();
        }

        private async Task LoadArmorsData()
        {
            Task<IList<ArmorPiecePrimitive>> armorTask = LoadBase<ArmorPiecePrimitive>(DB_KEY_ARMORS);
            var armorLocalizations = new Dictionary<string, Task<IList<ArmorPiecePrimitive>>>();
            foreach (KeyValuePair<string, string> localization in localizationKeys)
            {
                armorLocalizations[localization.Value] = LoadBase<ArmorPiecePrimitive>(localization.Key + "/" + DB_KEY_ARMORS);
            }
            Task<IList<ArmorSetPrimitive>> setsTask = LoadBase<ArmorSetPrimitive>(DB_KEY_ARMOR_SETS);
            var armorSetLocalizations = new Dictionary<string, Task<IList<ArmorSetPrimitive>>>();
            foreach (KeyValuePair<string, string> localization in localizationKeys)
            {
                armorSetLocalizations[localization.Value] = LoadBase<ArmorSetPrimitive>(localization.Key + "/" + DB_KEY_ARMOR_SETS);
            }

            IList <ArmorPiecePrimitive> armorResult = await armorTask;

            if (armorResult == null)
            {
                logger?.LogError("Query for all armors failed");
                return;
            }

            var allArmors = new Dictionary<int, DataStructures.ArmorPiece>();

            foreach(ArmorPiecePrimitive primitive in armorResult)
            {
                IAbility[] localAbilities = primitive.Abilities.Select(x => GetAbility(x.SkillId, x.Level)).ToArray();
                if (allArmors.ContainsKey(primitive.Id))
                    throw new InvalidOperationException($"Armor identifier with ID '{primitive.Id}' and Name '{primitive.Name}' is a duplicate");
                else
                    allArmors[primitive.Id] = new DataStructures.ArmorPiece(primitive, localAbilities, GetCraftingMaterials(primitive.Crafting.Materials));
            }

            foreach (KeyValuePair<string, Task<IList<ArmorPiecePrimitive>>> localizationQuery in armorLocalizations)
            {
                IList<ArmorPiecePrimitive> localization = await localizationQuery.Value;
                if (localization == null)
                {
                    logger?.LogError($"Query for armor localization '{localizationQuery.Key}' failed");
                    continue;
                }
                foreach (ArmorPiecePrimitive armor in localization)
                {
                    allArmors[armor.Id].AddLocalization(localizationQuery.Key, armor);
                }
            }

            IList<ArmorSetPrimitive> setsResult = await setsTask;
            if(setsResult == null)
            {
                logger?.LogError("Query for all armor sets failed");
                return;
            }
            var allArmorSetBoni = new Dictionary<int, Tuple<ArmorSetBonus, List<IArmorPiece>>>();
            int armorSetSkillpartId = 0;

            foreach (ArmorSetPrimitive armorSetPrimitive in setsResult)
            {
                var setArmorPieces = new List<IArmorPiece>();
                foreach(ArmorPieceIdPrimitive primitive in armorSetPrimitive.ArmorPieces)
                {
                    DataStructures.ArmorPiece armor = allArmors[primitive.ArmorPieceId];
                    armor.Id = armorSetPrimitive.Id; // The UI later groups the armors by their ID
                    setArmorPieces.Add(armor);
                }
                
                if (armorSetPrimitive.Bonus != null)
                {
                    var armorSetSkills = new List<IArmorSetSkillPart>();
                    foreach (ArmorSetBonusRankPrimitive bonusRank in armorSetPrimitive.Bonus.Ranks)
                    {
                        IAbility[] setAbilities = abilities.Where(a => a.Skill.Id == bonusRank.Skill.SkillId && a.Level == bonusRank.Skill.Level).ToArray();
                        armorSetSkills.Add(new ArmorSetSkillPart(armorSetSkillpartId, bonusRank.PieceCount, setAbilities));
                        ++armorSetSkillpartId;
                    }
                    var armorSetBonus = new ArmorSetBonus(armorSetPrimitive.Bonus, armorSetSkills.ToArray());
                    allArmorSetBoni[armorSetPrimitive.Id] = new Tuple<ArmorSetBonus, List<IArmorPiece>>(armorSetBonus, setArmorPieces);
                }

                // Infos about complete Armor sets can not be found in the MWH-DB
                //var armorSet = new FullArmorSet(armorSetPrimitive.Id, setArmorPieces.ToArray());

                //foreach (DataStructures.ArmorPiece armorPiece in setArmorPieces)
                //    armorPiece.FullArmorSet = armorSet;
            }
            foreach(KeyValuePair<string, Task<IList<ArmorSetPrimitive>>> task in armorSetLocalizations)
            {
                IList<ArmorSetPrimitive> localization = await task.Value;
                if (localization == null)
                {
                    logger?.LogError($"Query for armor sets localization '{task.Key}' failed");
                    continue;
                }
                foreach (ArmorSetPrimitive armorSet in localization)
                {
                    if (armorSet.Bonus == null)
                        continue;
                    if (allArmorSetBoni.ContainsKey(armorSet.Id))
                        allArmorSetBoni[armorSet.Id].Item1.AddLocalization(task.Key, armorSet.Bonus);
                    else
                        logger?.LogError($"Armor set with ID '{armorSet.Id}' in language '{task.Key}' does not exist in the original");
                }
            }
            foreach(KeyValuePair<int, Tuple<ArmorSetBonus, List<IArmorPiece>>> armorSetBonus in allArmorSetBoni)
            {
                foreach(DataStructures.ArmorPiece armor in armorSetBonus.Value.Item2)
                {
                    var boni = new IArmorSetSkill[1];
                    boni[0] = armorSetBonus.Value.Item1;
                    armor.UpdateArmorSetBoni(boni);
                }
            }

            armors = allArmors.Values.ToArray();
        }

        private async Task LoadSkillsData()
        {
            Task<IList<SkillPrimitive>> query = LoadBase<SkillPrimitive>(DB_KEY_SKILLS);
            var localizationQueries = new Dictionary<string, Task<IList<SkillPrimitive>>>();
            foreach(KeyValuePair<string, string> localization in localizationKeys)
            {
                localizationQueries[localization.Value] = LoadBase<SkillPrimitive>(localization.Key + "/" + DB_KEY_SKILLS);
            }


            var allSkills = new Dictionary<int, DataStructures.Skill>();

            IList<SkillPrimitive> result = await query;
            if (result == null)
            {
                logger?.LogError("Query for all skills failed");
                return;
            }
            foreach (SkillPrimitive skillPrimitive in result)
            {
                var skill = new DataStructures.Skill(skillPrimitive);

                if(allSkills.ContainsKey(skill.Id))
                    logger?.LogError($"Skill identifier with ID '{skill.Id}' and Name '{skill.Name}' is a duplicate");
                else
                    allSkills[skill.Id] = skill;
            }

            foreach(KeyValuePair<string, Task<IList<SkillPrimitive>>> localizationQuery in localizationQueries)
            {
                IList<SkillPrimitive> localization = await localizationQuery.Value;
                if(localization == null)
                {
                    logger?.LogError($"Query for skills localization '{localizationQuery.Key}' failed");
                    continue;
                }
                foreach(SkillPrimitive skill in localization)
                {
                    allSkills[skill.Id].AddLocalization(localizationQuery.Key, skill);
                }
            }

            skills = allSkills.Values.ToArray();
            var allAbilities = new HashSet<IAbility>();
            foreach (ISkill skill in skills)
            {
                foreach (DataStructures.Ability ability in skill.Abilities)
                {
                    if (allAbilities.Add(ability) == false)
                        logger?.LogError($"Ability identifier 'skill {ability.Skill.Name} level {ability.Level}' is a duplicate");
                }
            }
            abilities = allAbilities.ToArray();
        }

        private async Task LoadCharmsData()
        {
            var localizationQueries = new Dictionary<string, Task<IList<CharmPrimitive>>>();
            Task<IList<CharmPrimitive>> query = LoadBase<CharmPrimitive>(DB_KEY_CHARMS);
            foreach (KeyValuePair<string, string> localization in localizationKeys)
            {
                localizationQueries[localization.Value] = LoadBase<CharmPrimitive>(localization.Key + "/" + DB_KEY_CHARMS);
            }


            IList<CharmPrimitive> result = await query;

            if (result == null)
            {
                logger?.LogError("Query for all charms failed");
                return;
            }

            var localCharms = new Dictionary<int, DataStructures.Charm>();
            int currentCharmLevelId = 0;
            foreach(CharmPrimitive charmPrimitive in result)
            {
                var charm = new DataStructures.Charm(charmPrimitive);
                var charmLevels = new DataStructures.CharmLevel[charmPrimitive.Levels.Count];
                for (int j = 0; j < charmLevels.Length; j++)
                {
                    IAbility[] localAbilities = charmPrimitive.Levels[j].Abilitites.Select(x => GetAbility(x.SkillId, x.Level)).ToArray();
                    charmLevels[j] = new DataStructures.CharmLevel(currentCharmLevelId, charmPrimitive.Levels[j], localAbilities, GetCraftingMaterials(charmPrimitive.Levels[j].Crafting.Materials));
                    ++currentCharmLevelId;
                }
                charm.SetCharmLevels(charmLevels);
                if (localCharms.ContainsKey(charm.Id))
                    logger?.LogError($"Charm identifier with ID '{charm.Id}' and Name '{charm.Name}' is a duplicate");
                else
                    localCharms[charm.Id] = charm;
            }


            foreach (KeyValuePair<string, Task<IList<CharmPrimitive>>> localizationQuery in localizationQueries)
            {
                IList<CharmPrimitive> localization = await localizationQuery.Value;
                if (localization == null)
                {
                    logger?.LogError($"Query for charms localization '{localizationQuery.Key}' failed");
                    continue;
                }
                foreach (CharmPrimitive charm in localization)
                {
                    localCharms[charm.Id].AddLocalization(localizationQuery.Key, charm);
                }
            }

            charms = localCharms.Values.ToArray();
        }

        private async Task LoadJewelsData()
        {
            var localizationQueries = new Dictionary<string, Task<IList<JewelPrimitive>>>();
            Task<IList<JewelPrimitive>> query = LoadBase<JewelPrimitive>(DB_KEY_DECORATIONS);
            foreach (KeyValuePair<string, string> localization in localizationKeys)
            {
                localizationQueries[localization.Value] = LoadBase<JewelPrimitive>(localization.Key + "/" + DB_KEY_DECORATIONS);
            }


            IList<JewelPrimitive> result = await query;

            if (result == null)
            {
                logger?.LogError("Query for all decorations failed");
                return;
            }

            var localJewels = new Dictionary<int, DataStructures.Jewel>();

            foreach(JewelPrimitive jewelPrimitive in result)
            {
                IAbility[] localAbilities = jewelPrimitive.Abilitites.Select(a => GetAbility(a.SkillId, a.Level)).ToArray();
                if (localJewels.ContainsKey(jewelPrimitive.Id))
                    logger?.LogError($"Decoration identifier with ID '{jewelPrimitive.Id}' and Name '{jewelPrimitive.Name}' is a duplicate");
                else
                    localJewels[jewelPrimitive.Id] = new DataStructures.Jewel(jewelPrimitive, localAbilities);
            }

            foreach(KeyValuePair<string, Task<IList<JewelPrimitive>>> localizationQuery in localizationQueries)
            {
                IList<JewelPrimitive> localization = await localizationQuery.Value;
                if (localization == null)
                {
                    logger?.LogError($"Query for decoration localization '{localizationQuery.Key}' failed");
                    continue;
                }
                foreach (JewelPrimitive jewel in localization)
                {
                    localJewels[jewel.Id].AddLocalization(localizationQuery.Key, jewel);
                }
            }

            jewels = localJewels.Values.ToArray();
        }

        private ICraftMaterial[] GetCraftingMaterials(CraftingCostPrimitive[] primitives)
        {
            var materials = new ICraftMaterial[primitives.Length];
            for(int i = 0; i< primitives.Length;++i)
            {
                materials[i] = new CraftMaterial(localizedItems.FirstOrDefault(x => x.Id == primitives[i].Item.Id), primitives[i].Quantity);
            }
            return materials;
        }

        private IAbility GetAbility(int skillId, int level)
        {
            return abilities.FirstOrDefault(a => a.Skill.Id == skillId && a.Level == level);
        }
    }
}
