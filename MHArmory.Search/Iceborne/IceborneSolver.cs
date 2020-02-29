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
    public class IceborneSolver : SolverBase, IDisposable
    {
        public override string Name { get; } = "Iceborne - Default";
        public new string Author { get; } = "ChaosSaber";
        public override string Description { get; } = "Default Search for Iceborne Data";
        public override int Version { get; } = 1;

        private Dictionary<ISkill, DecoCollection> decos = new Dictionary<ISkill, DecoCollection>();
        private List<ISkill> noJewelSkills;
        private List<ISkill> noLevel4DecoSkills;
        private List<ISkill> onlyLevel4DecoSkills;
        private List<ISkill> level4DecoSkills;
        private List<ISkill>[] multiLevelDecoSkills = new List<ISkill>[3];
        private Dictionary<ISkill, int> desiredAbilities;

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

        private void GetArmorSkills(IEquipment[] equips, AbilityMap abilities)
        {
            Dictionary<IArmorSetSkillPart, int> armorSetSkillParts = armorSetSkillPartsObjectPool.GetObject();
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

                foreach (IArmorSetSkill armorSetSkill in armorPiece.ArmorSetSkills)
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
         *      Check if every skill is present where no decos are available.
         *      Inserts all decos which have no multiskill/multilevel decos.
         *      Inserts all decos which have only multiskill/multilevel decos.
         *      Inserts all decos which have 4 points in a single skill.
         *      Inserts all decos which have 3 points in a single skill.
         *      Inserts all decos which have 2 points in a single skill.
         *      Inserts all decos with multiple skills in the following order:
         *          Decos with another unfullfilled skill.
         *          Decos which can improve another desired skill.
         *      Inserts remaining generic decos .
         *      Inserts single skill single level decos.
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

            /**
             * returns true if deco could be added, otherwise false
             */
            int AddDeco(ref int remainingLevels, IJewel deco, int skillLevels, int count, bool limitToExactSlotsize = false)
            {
                int slotted = SolverUtils.ConsumeSlots(availableSlots, deco.SlotSize, count, limitToExactSlotsize);
                if (slotted > 0)
                {
                    remainingLevels -= skillLevels * slotted;
                    usedDecos.AddDecos(deco, slotted);
                    abilities.AddDecos(deco, slotted);
                }
                return slotted;
            }

            void AddMultiLevelDeco(ISkill skill, SolverDataJewelModel deco, ref int remainingLevels, bool canOvershoot = false)
            {
                if (deco == null)
                    return;

                int remainingDecos = deco.Available - usedDecos[deco.Jewel];
                if (remainingDecos == 0)
                    return;

                int skillLevels = deco.Jewel.Abilities[0].Level;
                int decosNeeded = remainingLevels / skillLevels;
                if (decosNeeded > 0)
                {
                    int use = Math.Min(decosNeeded, remainingDecos);
                    if (AddDeco(ref remainingLevels, deco.Jewel, skillLevels, use) != use)
                        return;
                    if (remainingDecos == use || remainingLevels == 0)
                        return;
                }

                if (canOvershoot ||
                   (remainingLevels > 1 && skill.MaxLevel > desiredAbilities[skill]))
                    AddDeco(ref remainingLevels, deco.Jewel, skillLevels, 1);
            }

            void AddMultiLevelDecos(DecoCollection decoCollection, ref int remainingLevels, bool canOvershoot = false)
            {
                for (int i = 4; i > 1 && remainingLevels > 0; --i)
                {
                    AddMultiLevelDeco(decoCollection.Skill, decoCollection.SingleSkillDecos[i - 1], ref remainingLevels, true);
                }
            }

            void AddMultiskillDeco(ISkill skill, SolverDataJewelModel deco, ref int remainingLevels, bool canImproveOtherSkill = false)
            {
                if (deco == null)
                    return;

                int remainingDecos = deco.Available - usedDecos[deco.Jewel];
                if (remainingDecos == 0)
                    return;

                ISkill otherSkill = deco.Jewel.Abilities.First(a => a.Skill != skill).Skill;
                int remainingLevelsOtherSkill = GetRemainingSkillLevels(otherSkill);
                if (remainingLevelsOtherSkill > 0)
                {
                    int use = Math.Min(remainingLevels, Math.Min(remainingLevelsOtherSkill, remainingDecos));
                    if (AddDeco(ref remainingLevels, deco.Jewel, 1, use) != use)
                        return;
                    if (use == remainingDecos || remainingLevels == 0)
                        return;
                    remainingDecos -= use;
                }

                if (canImproveOtherSkill)
                {
                    int improvableOtherSkillLevels = otherSkill.MaxLevel - abilities[otherSkill];
                    if (improvableOtherSkillLevels > 0)
                    {
                        int use = Math.Min(remainingLevels, Math.Min(remainingDecos, improvableOtherSkillLevels));
                        if (AddDeco(ref remainingLevels, deco.Jewel, 1, use) != use)
                            return;
                    }
                }
            }

            void AddMultiSkillDecos(DecoCollection decoCollection, ref int remainingLevels, bool canImproveOtherSkill = false)
            {
                foreach (SolverDataJewelModel deco in decoCollection.MultiSkillDecos)
                {
                    AddMultiskillDeco(decoCollection.Skill, deco, ref remainingLevels, true);
                    if (remainingLevels <= 0)
                        break;
                }
            }

            void AddGenericDecos(DecoCollection decoCollection, ref int remainingLevels)
            {
                SolverDataJewelModel genericDeco = decoCollection.GenericLevel4Deco;
                int remainingDecos = genericDeco.Available - usedDecos[genericDeco.Jewel];
                int use = Math.Min(remainingDecos, remainingLevels);
                if (AddDeco(ref remainingLevels, genericDeco.Jewel, 1, use) != use)
                    return;

                if (remainingLevels <= 0)
                    return;

                foreach (SolverDataJewelModel deco in decoCollection.MultiSkillDecos)
                {
                    int remaining = deco.Available - usedDecos[deco.Jewel];
                    if (remaining == 0)
                        continue;

                    int use2 = Math.Min(remaining, remainingLevels);
                    int slotted = SolverUtils.ConsumeSlots(availableSlots, deco.Jewel.SlotSize, use);
                    if(slotted > 0)
                    {
                        remainingLevels -= slotted;
                        usedDecos.AddDecos(deco.Jewel, slotted);
                        abilities.AddDecos(genericDeco.Jewel, slotted);
                    }

                    if (remainingLevels < 0)
                        return;
                }
            }

            void AddBelowLevel4Decos(ISkill skill, SolverDataJewelModel deco, ref int remainingLevels, bool limitToExactSlotsize = false)
            {
                if (deco == null)
                    return;
                int remainingDecos = deco.Available - usedDecos[deco.Jewel];
                if (remainingDecos == 0)
                    return;

                int use = Math.Min(remainingDecos, remainingLevels);
                AddDeco(ref remainingLevels, deco.Jewel, 1, use, limitToExactSlotsize);
            }

            SolverUtils.AccumulateAvailableSlots(weapon, availableSlots);
            foreach (IEquipment equipment in equips)
                SolverUtils.AccumulateAvailableSlots(equipment, availableSlots);

            GetArmorSkills(equips, abilities);

            foreach (ISkill skill in noJewelSkills)
            {
                if (!IsAbilityFullfilled(skill))
                {
                    OnArmorSetMismatch();
                    return ArmorSetSearchResult.NoMatch;
                }
            }

            foreach (ISkill skill in noLevel4DecoSkills)
            {
                int remainingLevels = GetRemainingSkillLevels(skill);
                if (remainingLevels <= 0)
                    continue;

                SolverDataJewelModel jewel = decos[skill].SingleSkillDecos[0];
                if (jewel.Available < remainingLevels)
                {
                    OnArmorSetMismatch();
                    return ArmorSetSearchResult.NoMatch;
                }

                int tmp = remainingLevels;
                if(AddDeco(ref remainingLevels, jewel.Jewel, 1, remainingLevels) != tmp)
                {
                    OnArmorSetMismatch();
                    return ArmorSetSearchResult.NoMatch;
                }
            }

            foreach (ISkill skill in onlyLevel4DecoSkills)
            {
                int remainingLevels = GetRemainingSkillLevels(skill);
                if (remainingLevels <= 0)
                    continue;

                DecoCollection decoCollection = decos[skill];

                AddMultiLevelDecos(decoCollection, ref remainingLevels);
                if (remainingLevels <= 0)
                    continue;

                AddMultiSkillDecos(decoCollection, ref remainingLevels);
                if (remainingLevels <= 0)
                    continue;

                AddMultiSkillDecos(decoCollection, ref remainingLevels, true);
                if (remainingLevels <= 0)
                    continue;

                AddMultiLevelDecos(decoCollection, ref remainingLevels, true);
                if (remainingLevels <= 0)
                    continue;

                AddGenericDecos(decoCollection, ref remainingLevels);
                if (remainingLevels > 0)
                {
                    OnArmorSetMismatch();
                    return ArmorSetSearchResult.NoMatch;
                }
            }

            for (int i = 4; i > 1; --i)
            {
                foreach (ISkill skill in multiLevelDecoSkills[i - 2])
                {
                    int remainingLevels = GetRemainingSkillLevels(skill);
                    if (remainingLevels <= 0)
                        continue;
                    AddMultiLevelDeco(skill, decos[skill].SingleSkillDecos[i - 1], ref remainingLevels);
                }
            }

            foreach(ISkill skill in level4DecoSkills)
            {
                int remainingLevels = GetRemainingSkillLevels(skill);
                if (remainingLevels <= 0)
                    continue;

                DecoCollection decoCollection = decos[skill];

                AddMultiSkillDecos(decoCollection, ref remainingLevels);
                if (remainingLevels <= 0)
                    continue;

                AddBelowLevel4Decos(skill, decoCollection.SingleSkillDecos[0], ref remainingLevels, true);
            }

            foreach(ISkill skill in level4DecoSkills)
            {
                if(SolverUtils.AreAllSlotsUsed(availableSlots))
                {
                    OnArmorSetMismatch();
                    return ArmorSetSearchResult.NoMatch;
                }

                int remainingLevels = GetRemainingSkillLevels(skill);
                if (remainingLevels <= 0)
                    continue;

                DecoCollection decoCollection = decos[skill];

                AddMultiSkillDecos(decoCollection, ref remainingLevels, true);
                if (remainingLevels <= 0)
                    continue;

                AddBelowLevel4Decos(skill, decoCollection.SingleSkillDecos[0], ref remainingLevels, false);
                if (remainingLevels <= 0)
                    continue;

                AddMultiLevelDecos(decoCollection, ref remainingLevels, true);
                if (remainingLevels <= 0)
                    continue;

                AddGenericDecos(decoCollection, ref remainingLevels);

                if(remainingLevels > 0)
                {
                    OnArmorSetMismatch();
                    return ArmorSetSearchResult.NoMatch;
                }
            }

            // Sanity Check for testing
            foreach(KeyValuePair<ISkill, int> pair in desiredAbilities)
            {
                if (!IsAbilityFullfilled(pair.Key))
                {
                    throw new InvalidProgramException("Sanity Check failed");
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

            noJewelSkills = decos.Values
                .Where(x => x.TotalSkillLevels == 0)
                .Select(x => x.Skill)
                .ToList();

            noLevel4DecoSkills = decos.Values
                .Where(x => x.TotalSkillLevels > 0 && x.HasLevel4Deco == false)
                .OrderByDescending(x => x.SingleSkillDecos[0].Jewel.SlotSize)
                .Select(x => x.Skill)
                .ToList();

            onlyLevel4DecoSkills = decos.Values
                .Where(x => x.HasLevel4Deco && x.SingleSkillDecos[0] == null)
                .Select(x => x.Skill)
                .ToList();

            level4DecoSkills = decos.Values
                .Where(x => x.HasLevel4Deco && x.SingleSkillDecos[0] != null)
                .OrderByDescending(x => x.SingleSkillDecos[0].Jewel.SlotSize)
                .Select(x => x.Skill)
                .ToList();

            for (int i = 4; i > 1; --i)
            {
                multiLevelDecoSkills[i - 2] = decos.Values
                    .Where(j => j.HasLevel4Deco && j.SingleSkillDecos[i - 1] != null)
                    .OrderByDescending(x => x.SingleSkillDecos[0]?.Jewel.SlotSize)
                    .Select(x => x.Skill)
                    .ToList();
            }

            var combinations = decos.Values
                .Where(x => x.MultiSkillDecos.Count > 0)
                .ToDictionary(x => x.Skill, x => x.MultiSkillDecos.Count);

            foreach (KeyValuePair<ISkill, DecoCollection> decoPair in decos)
                decoPair.Value.SortMultiSkillDecos(combinations);
        }
    }
}
