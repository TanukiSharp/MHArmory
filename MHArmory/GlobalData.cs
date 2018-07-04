using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.ViewModels;

namespace MHArmory
{
    public class GlobalData
    {
        public static readonly GlobalData Instance = new GlobalData();

        // ================================================================================================================

        #region Abilities

        private readonly TaskCompletionSource<IList<AbilityViewModel>> abilitiesTaskCompletionSource = new TaskCompletionSource<IList<AbilityViewModel>>();

        public void SetAbilities(IList<AbilityViewModel> abilities)
        {
            abilitiesTaskCompletionSource.TrySetResult(abilities);
        }

        public Task<IList<AbilityViewModel>> GetAbilities()
        {
            return abilitiesTaskCompletionSource.Task;
        }

        #endregion // Abilities

        // ================================================================================================================

        #region Skills

        private readonly TaskCompletionSource<IList<SkillViewModel>> skillsTaskCompletionSource = new TaskCompletionSource<IList<SkillViewModel>>();

        public void SetSkills(IList<SkillViewModel> skills)
        {
            skillsTaskCompletionSource.TrySetResult(skills);
        }

        public Task<IList<SkillViewModel>> GetSkills()
        {
            return skillsTaskCompletionSource.Task;
        }

        #endregion // Skills

        // ================================================================================================================

        #region Armors

        private readonly TaskCompletionSource<object> armorsTaskCompletionSource = new TaskCompletionSource<object>();

        private IList<IArmorPiece> heads;
        private IList<IArmorPiece> chests;
        private IList<IArmorPiece> gloves;
        private IList<IArmorPiece> waists;
        private IList<IArmorPiece> legs;

        public void SetArmors(IArmorPiece[] armorPieces)
        {
            heads = ArmorUtility.ExcludeLessPoweredEquivalent(armorPieces.Where(x => x.Type == ArmorPieceType.Head).ToList());
            chests = ArmorUtility.ExcludeLessPoweredEquivalent(armorPieces.Where(x => x.Type == ArmorPieceType.Chest).ToList());
            gloves = ArmorUtility.ExcludeLessPoweredEquivalent(armorPieces.Where(x => x.Type == ArmorPieceType.Gloves).ToList());
            waists = ArmorUtility.ExcludeLessPoweredEquivalent(armorPieces.Where(x => x.Type == ArmorPieceType.Waist).ToList());
            legs = ArmorUtility.ExcludeLessPoweredEquivalent(armorPieces.Where(x => x.Type == ArmorPieceType.Legs).ToList());

            armorsTaskCompletionSource.TrySetResult(null);
        }

        public async Task<IList<IArmorPiece>> GetHeads()
        {
            await armorsTaskCompletionSource.Task;
            return heads;
        }

        public async Task<IList<IArmorPiece>> GetChests()
        {
            await armorsTaskCompletionSource.Task;
            return chests;
        }

        public async Task<IList<IArmorPiece>> GetGloves()
        {
            await armorsTaskCompletionSource.Task;
            return gloves;
        }

        public async Task<IList<IArmorPiece>> GetWaists()
        {
            await armorsTaskCompletionSource.Task;
            return waists;
        }

        public async Task<IList<IArmorPiece>> GetLegs()
        {
            await armorsTaskCompletionSource.Task;
            return legs;
        }

        #endregion // Armors

        // ================================================================================================================

        #region SkillsToArmorsMap

        private readonly TaskCompletionSource<IDictionary<int, IList<IArmorPiece>>> skillsToArmorsMapTaskCompletionSource = new TaskCompletionSource<IDictionary<int, IList<IArmorPiece>>>();

        public void SetSkillsToArmorsMap(IDictionary<int, IList<IArmorPiece>> skillsToArmorsMap)
        {
            skillsToArmorsMapTaskCompletionSource.TrySetResult(skillsToArmorsMap);
        }

        public Task<IDictionary<int, IList<IArmorPiece>>> GetSkillsToArmorsMap()
        {
            return skillsToArmorsMapTaskCompletionSource.Task;
        }

        #endregion // SkillsToArmorsMap

        // ================================================================================================================

        #region SkillsToCharmsMap

        private readonly TaskCompletionSource<IDictionary<int, IList<ICharm>>> skillsToCharmsMapTaskCompletionSource = new TaskCompletionSource<IDictionary<int, IList<ICharm>>>();

        public void SetSkillsToCharmsMap(IDictionary<int, IList<ICharm>> skillsToCharmsMap)
        {
            skillsToCharmsMapTaskCompletionSource.TrySetResult(skillsToCharmsMap);
        }

        public Task<IDictionary<int, IList<ICharm>>> GetSkillsToCharmsMap()
        {
            return skillsToCharmsMapTaskCompletionSource.Task;
        }

        #endregion // SkillsToCharmsMap

        // ================================================================================================================

        #region SkillsToJewelsMap

        private readonly TaskCompletionSource<IDictionary<int, IList<IJewel>>> skillsToJewelsMapTaskCompletionSource = new TaskCompletionSource<IDictionary<int, IList<IJewel>>>();

        public void SetSkillsToJewelsMap(IDictionary<int, IList<IJewel>> skillsToJewelsMap)
        {
            skillsToJewelsMapTaskCompletionSource.TrySetResult(skillsToJewelsMap);
        }

        public Task<IDictionary<int, IList<IJewel>>> GetSkillsToJewelsMap()
        {
            return skillsToJewelsMapTaskCompletionSource.Task;
        }

        #endregion // SkillsToJewelsMap
    }
}
