using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;
using MHArmory.Search.Default;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MHArmory.Search.Iceborne
{
    class IceborneSolver : SolverBase, IDisposable
    {
        public override string Name { get; } = "Iceborne - Default";
        public new string Author { get; } = "ChaosSaber";
        public override string Description { get; } = "Default Search for Iceborne Data";
        public override int Version { get; } = 1;

        private Dictionary<ISkill, DecoCollection> jewels = new Dictionary<ISkill, DecoCollection>();
        private List<IAbility> noJewelAbilities;
        private List<IAbility> noLevel4JewelAbilities;
        private List<IAbility> onlyLevel4JewelAbilities;
        private List<IAbility> level4JewelAbilities;
        private List<IAbility>[] multilevelSkillJewelAbilities = new List<IAbility>[3];

        public void Dispose()
        {
            jewelResultObjectPool.Dispose();
            availableSlotsObjectPool.Dispose();
            abilitiesObjectPool.Dispose();
        }

        private readonly ObjectPool<Dictionary<int, SolverDataJewelModel>> jewelResultObjectPool =
            new ObjectPool<Dictionary<int, SolverDataJewelModel>>(() => new Dictionary<int, SolverDataJewelModel>());
        private readonly ObjectPool<int[]> availableSlotsObjectPool = new ObjectPool<int[]>(() => new int[4]);
        private readonly ObjectPool<Dictionary<ISkill, int>> abilitiesObjectPool =
            new ObjectPool<Dictionary<ISkill, int>>(() => new Dictionary<ISkill, int>());

        private void ReturnSlotsArray(int[] slotsArray)
        {
            for (int i = 0; i < slotsArray.Length; i++)
                slotsArray[i] = 0;

            availableSlotsObjectPool.PutObject(slotsArray);
        }

        protected override ArmorSetSearchResult IsArmorSetMatching(IEquipment weapon, IEquipment[] equips)
        {
            Dictionary<int, SolverDataJewelModel> requiredJewels = jewelResultObjectPool.GetObject();
            int[] availableSlots = availableSlotsObjectPool.GetObject();
            Dictionary<ISkill, int> abilities = abilitiesObjectPool.GetObject();

            void OnArmorSetMismatch()
            {
                requiredJewels.Clear();
                jewelResultObjectPool.PutObject(requiredJewels);

                abilities.Clear();
                abilitiesObjectPool.PutObject(abilities);

                ReturnSlotsArray(availableSlots);
            }

            SolverUtils.AccumulateAvailableSlots(weapon, availableSlots);
            foreach (IEquipment equipment in equips)
                SolverUtils.AccumulateAvailableSlots(equipment, availableSlots);

            foreach (IEquipment equipment in equips)
            {
                if (equipment == null)
                    continue;

                foreach (IAbility a in equipment.Abilities)
                {
                    if (abilities.ContainsKey(a.Skill))
                        abilities[a.Skill] += a.Level;
                    else
                        abilities[a.Skill] = a.Level;
                }
            }

            foreach (IAbility ability in noJewelAbilities)
            {

                if (!abilities.ContainsKey(ability.Skill) || ability.Level > abilities[ability.Skill])
                {
                    OnArmorSetMismatch();
                    return ArmorSetSearchResult.NoMatch;
                }
            }
        }

        protected override void OnSearchBegin(SolverDataJewelModel[] matchingJewels, IAbility[] desiredAbilities)
        {
            foreach (IAbility ability in desiredAbilities)
            {
                jewels[ability.Skill] = new DecoCollection(matchingJewels.Where(j => j.Jewel.Abilities.Any(a => a.Skill == ability.Skill)), ability);
            }

            noJewelAbilities = jewels.Values
                .Where(x => x.TotalSkillLevels == 0)
                .Select(x => desiredAbilities.First(a => a.Skill == x.Skill))
                .ToList();

            noLevel4JewelAbilities = jewels.Values
                .Where(x => x.TotalSkillLevels > 0 && x.HasLevel4Deco == false)
                .OrderByDescending(x => x.SingleSkillDecos[0].Jewel.SlotSize)
                .Select(x => desiredAbilities.First(a => a.Skill == x.Skill))
                .ToList();

            onlyLevel4JewelAbilities = jewels.Values
                .Where(x => x.HasLevel4Deco && x.SingleSkillDecos[0] == null)
                .Select(x => desiredAbilities.First(a => a.Skill == x.Skill))
                .ToList();

            level4JewelAbilities = jewels.Values
                .Where(x => x.HasLevel4Deco)             
                .Select(x => desiredAbilities.First(a => a.Skill == x.Skill))
                .ToList();

            for(int i = 4; i > 1; --i)
            {
                multilevelSkillJewelAbilities[i - 2] = jewels.Values
                    .Where(j => j.HasLevel4Deco && j.SingleSkillDecos[i - 1] != null)
                    .Select(x => desiredAbilities.First(a => a.Skill == x.Skill))
                    .ToList();
            }
        }
    }
}
