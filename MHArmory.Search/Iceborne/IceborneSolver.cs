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
    public sealed class IceborneSolver : SolverBase, IDisposable
    {
        public override string Name { get; } = "Iceborne - Default";
        public new string Author { get; } = "ChaosSaber";
        public override string Description { get; } = "Default search algorithm for MR";
        public override int Version { get; } = 1;

        private Dictionary<ISkill, DecoCollection> decos = new Dictionary<ISkill, DecoCollection>();
        private Dictionary<ISkill, int> desiredAbilities;
        private List<SolverDataJewelModel> allDecos;

        public void Dispose()
        {
            usedDecosObjectPool.Dispose();
            availableSlotsObjectPool.Dispose();
            abilitiesObjectPool.Dispose();
            armorSetSkillPartsObjectPool.Dispose();
        }

        private readonly ObjectPool<DecoMap> usedDecosObjectPool = new ObjectPool<DecoMap>(() => new DecoMap());
        private readonly ObjectPool<int[]> availableSlotsObjectPool = new ObjectPool<int[]>(() => new int[4]);
        private readonly ObjectPool<AbilityMap> abilitiesObjectPool = new ObjectPool<AbilityMap>(() => new AbilityMap());
        private readonly ObjectPool<Dictionary<IArmorSetSkillPart, int>> armorSetSkillPartsObjectPool =
            new ObjectPool<Dictionary<IArmorSetSkillPart, int>>(() => new Dictionary<IArmorSetSkillPart, int>(SolverUtils.ArmorSetSkillPartEqualityComparer.Default));

        private void ReturnSlotsArray(int[] slotsArray)
        {
            for (int i = 0; i < slotsArray.Length; i++)
                slotsArray[i] = 0;

            availableSlotsObjectPool.PutObject(slotsArray);
        }

        private void GetArmorSkills(IEquipment[] equips, AbilityMap abilities, IEquipment weapon)
        {
            Dictionary<IArmorSetSkillPart, int> armorSetSkillParts = armorSetSkillPartsObjectPool.GetObject();

            void AddArmorSetSkills(IArmorSetSkill[] setSkills)
            {

                foreach (IArmorSetSkill armorSetSkill in setSkills)
                {
                    foreach (IArmorSetSkillPart armorSetSkillPart in armorSetSkill.Parts)
                    {
                        if (armorSetSkillParts.ContainsKey(armorSetSkillPart))
                            ++armorSetSkillParts[armorSetSkillPart];
                        else
                            armorSetSkillParts[armorSetSkillPart] = 1;
                    }
                }
            }
            foreach (IEquipment equipment in equips)
            {
                if (equipment == null)
                    continue;

                foreach (IAbility a in equipment.Abilities)
                    abilities.AddAbility(a);

                var armorPiece = equipment as IArmorPiece;
                if (armorPiece == null)
                    continue;
                if (armorPiece.ArmorSetSkills == null)
                    continue;
                AddArmorSetSkills(armorPiece.ArmorSetSkills);
            }

            var weaponPiece = weapon as Weapon;
            AddArmorSetSkills(weaponPiece.ArmorSetSkills);

            foreach (KeyValuePair<IArmorSetSkillPart, int> armorSetSkillPartKeyValue in armorSetSkillParts.Where(p => p.Value >= p.Key.RequiredArmorPieces))
            {
                foreach (IAbility x in armorSetSkillPartKeyValue.Key.GrantedSkills)
                {
                    abilities.AddAbility(x);
                }
            }
            armorSetSkillParts.Clear();
            armorSetSkillPartsObjectPool.PutObject(armorSetSkillParts);
        }

        /**
         * This function checks that the given armor set can fit all the desired skills.
         * For this it will do the following:
         *      Gather all skills already present on the armor.
         *      Make a quick check if enough decos for every missing skillpoint is available.
         *      Try to fit the presorted deco list into the armour set.
         *          The first run Tries to not waste/improve skill points and a deco is limited to its own slotsize.
         *          The second run can waste/improve skill points and the deco is no longer limited to its own slotsize.
         *      Check if every desired Ability is satisfied.
         */
        protected override ArmorSetSearchResult IsArmorSetMatching(IEquipment weapon, IEquipment[] equips)
        {
            DecoMap usedDecos = usedDecosObjectPool.GetObject();
            int[] availableSlots = availableSlotsObjectPool.GetObject();
            AbilityMap abilities = abilitiesObjectPool.GetObject();

            void Done()
            {
                usedDecos.Clear();
                usedDecosObjectPool.PutObject(usedDecos);

                abilities.Clear();
                abilitiesObjectPool.PutObject(abilities);
            }

            void OnArmorSetMismatch()
            {
                Done();
                ReturnSlotsArray(availableSlots);
            }

            int GetRemainingSkillLevels(ISkill skill)
            {
                return desiredAbilities[skill] - abilities[skill];
            }

            bool IsAbilityFullfilled(ISkill skill)
            {
                return GetRemainingSkillLevels(skill) <= 0;
            }

            SolverUtils.AccumulateAvailableSlots(weapon, availableSlots);
            foreach (IEquipment equipment in equips)
                SolverUtils.AccumulateAvailableSlots(equipment, availableSlots);

            GetArmorSkills(equips, abilities, weapon);

            // Quickckeck if we have enough decos
            foreach (KeyValuePair<ISkill, int> skills in desiredAbilities)
            {
                int remaining = skills.Value - abilities[skills.Key];
                if (remaining <= 0)
                    continue;
                if (decos[skills.Key].TotalSkillLevels < remaining)
                {
                    OnArmorSetMismatch();
                    return ArmorSetSearchResult.NoMatch;
                }
            }

            /**
             * returns the number of decos slotted
             */
            int AddDeco(IJewel deco, int count, bool limitToExactSlotsize = false)
            {
                int slotted = SolverUtils.ConsumeSlots(availableSlots, deco.SlotSize, count, limitToExactSlotsize);
                if (slotted > 0)
                {
                    usedDecos.AddDecos(deco, slotted);
                    abilities.AddDecos(deco, slotted);
                }
                return slotted;
            }

            foreach (SolverDataJewelModel solverData in allDecos)
            {
                int min = Math.Min(solverData.Jewel.Abilities.Min(a => GetRemainingSkillLevels(a.Skill) / a.Level), solverData.Available);
                if (min <= 0)
                    continue;
                AddDeco(solverData.Jewel, min, true);
            }

            foreach (SolverDataJewelModel solverData in allDecos)
            {
                int min = Math.Min(solverData.Jewel.Abilities.Max(a => (int)Math.Ceiling(1.0 * GetRemainingSkillLevels(a.Skill) / a.Level)), solverData.Available - usedDecos[solverData.Jewel]);
                if (min <= 0)
                    continue;
                AddDeco(solverData.Jewel, min, false);
            }

            foreach (KeyValuePair<ISkill, int> pair in desiredAbilities)
            {
                if (!IsAbilityFullfilled(pair.Key))
                {
                    OnArmorSetMismatch();
                    return ArmorSetSearchResult.NoMatch;
                }
            }

            // Sanity Check for testing
            foreach (SolverDataJewelModel deco in allDecos)
            {
                if (usedDecos[deco.Jewel] > deco.Available)
                {
                    throw new InvalidProgramException("Sanity Check failed: more decos used than available");
                    //OnArmorSetMismatch();
                    //return ArmorSetSearchResult.NoMatch;
                }
            }


            var set = new ArmorSetSearchResult
            {
                IsMatch = true,
                Jewels = usedDecos.ToArmorSetJewelResult(),
                SpareSlots = availableSlots
            };
            Done();
            return set;
        }

        protected override void OnSearchBegin(SolverDataJewelModel[] matchingJewels, IAbility[] desiredAbilities)
        {
            foreach (IAbility ability in desiredAbilities)
            {
                decos[ability.Skill] = new DecoCollection(matchingJewels.Where(j => j.Jewel.Abilities.Any(a => a.Skill == ability.Skill)), ability);
            }

            this.desiredAbilities = desiredAbilities.ToDictionary(x => x.Skill, x => x.Level);
            var skillToSlotsize = decos.Values.ToDictionary(x => x.Skill, x => x.SingleSkillDecos[0] == null ? 4 : x.SingleSkillDecos[0].Jewel.SlotSize);

            allDecos = matchingJewels
                .Where(x => x.Available > 0)
                .OrderByDescending(x => x.Jewel.Abilities.Sum(a => a.Level))
                .ThenByDescending(x => x.Jewel.Abilities.Sum(a => skillToSlotsize[a.Skill] * a.Level))
                .ThenByDescending(x => x.Jewel.SlotSize)
                .ToList();

            var combinations = decos.Values
                .Where(x => x.MultiSkillDecos.Count > 0)
                .ToDictionary(x => x.Skill, x => x.MultiSkillDecos.Count);

            foreach (KeyValuePair<ISkill, DecoCollection> decoPair in decos)
                decoPair.Value.SortMultiSkillDecos(combinations);
        }
    }
}
