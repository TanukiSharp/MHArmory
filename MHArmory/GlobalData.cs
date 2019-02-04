using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.ViewModels;
using MHArmory.Configurations;

namespace MHArmory
{
    public class GlobalData
    {
        public static readonly GlobalData Instance = new GlobalData();

        public ConfigurationV3 Configuration;

        public IList<IAbility> Abilities { get; private set; }
        public IList<ISkill> Skills { get; private set; }
        public IList<ICharmLevel> Charms { get; set; }
        public IList<IJewel> Jewels { get; set; }

        public IList<IArmorPiece> Heads { get; private set; }
        public IList<IArmorPiece> Chests { get; private set; }
        public IList<IArmorPiece> Gloves { get; private set; }
        public IList<IArmorPiece> Waists { get; private set; }
        public IList<IArmorPiece> Legs { get; private set; }

        public IDictionary<string, string> Aliases { get; private set; }

        public void SetArmors(IArmorPiece[] armorPieces)
        {
            Heads = armorPieces.Where(x => x.Type == EquipmentType.Head).ToList();
            Chests = armorPieces.Where(x => x.Type == EquipmentType.Chest).ToList();
            Gloves = armorPieces.Where(x => x.Type == EquipmentType.Gloves).ToList();
            Waists = armorPieces.Where(x => x.Type == EquipmentType.Waist).ToList();
            Legs = armorPieces.Where(x => x.Type == EquipmentType.Legs).ToList();
        }

        public void SetSkills(IList<ISkill> skills)
        {
            Skills = skills;
            Abilities = skills.SelectMany(s => s.Abilities).Distinct().ToList();
        }

        public void SetAliases(IDictionary<string, string> aliases)
        {
            Aliases = aliases
                .Where(kv => kv.Key != null && kv.Value != null)
                .ToDictionary(x => x.Key.Trim().ToLower(), y => y.Value.Trim().ToLower());
        }

        //// ================================================================================================================

        //#region SkillsToArmorsMap

        //private readonly TaskCompletionSource<IDictionary<int, IList<IArmorPiece>>> skillsToArmorsMapTaskCompletionSource = new TaskCompletionSource<IDictionary<int, IList<IArmorPiece>>>();

        //public void SetSkillsToArmorsMap(IDictionary<int, IList<IArmorPiece>> skillsToArmorsMap)
        //{
        //    skillsToArmorsMapTaskCompletionSource.TrySetResult(skillsToArmorsMap);
        //}

        //public Task<IDictionary<int, IList<IArmorPiece>>> GetSkillsToArmorsMap()
        //{
        //    return skillsToArmorsMapTaskCompletionSource.Task;
        //}

        //#endregion // SkillsToArmorsMap

        //// ================================================================================================================

        //#region SkillsToCharmsMap

        //private readonly TaskCompletionSource<IDictionary<int, IList<ICharm>>> skillsToCharmsMapTaskCompletionSource = new TaskCompletionSource<IDictionary<int, IList<ICharm>>>();

        //public void SetSkillsToCharmsMap(IDictionary<int, IList<ICharm>> skillsToCharmsMap)
        //{
        //    skillsToCharmsMapTaskCompletionSource.TrySetResult(skillsToCharmsMap);
        //}

        //public Task<IDictionary<int, IList<ICharm>>> GetSkillsToCharmsMap()
        //{
        //    return skillsToCharmsMapTaskCompletionSource.Task;
        //}

        //#endregion // SkillsToCharmsMap

        //// ================================================================================================================

        //#region SkillsToJewelsMap

        //private readonly TaskCompletionSource<IDictionary<int, IList<IJewel>>> skillsToJewelsMapTaskCompletionSource = new TaskCompletionSource<IDictionary<int, IList<IJewel>>>();

        //public void SetSkillsToJewelsMap(IDictionary<int, IList<IJewel>> skillsToJewelsMap)
        //{
        //    skillsToJewelsMapTaskCompletionSource.TrySetResult(skillsToJewelsMap);
        //}

        //public Task<IDictionary<int, IList<IJewel>>> GetSkillsToJewelsMap()
        //{
        //    return skillsToJewelsMapTaskCompletionSource.Task;
        //}

        //#endregion // SkillsToJewelsMap
    }
}
