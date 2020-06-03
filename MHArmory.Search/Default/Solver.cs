using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;

namespace MHArmory.Search.Default
{
    public sealed class Solver : SolverBase, IDisposable
    {
        public override string Name { get; } = "Armory - Default";
        public override string Description { get; } = "Default search algorithm (only use for LR and HR)";
        public override int Version { get; } = 1;

        public void Dispose()
        {
            jewelResultObjectPool.Dispose();
            availableSlotsObjectPool.Dispose();
            armorSetSkillPartsObjectPool.Dispose();
        }

        private readonly ObjectPool<List<ArmorSetJewelResult>> jewelResultObjectPool = new ObjectPool<List<ArmorSetJewelResult>>(() => new List<ArmorSetJewelResult>());
        private readonly ObjectPool<int[]> availableSlotsObjectPool = new ObjectPool<int[]>(() => new int[4]);
        private readonly ObjectPool<Dictionary<IArmorSetSkillPart, int>> armorSetSkillPartsObjectPool =
            new ObjectPool<Dictionary<IArmorSetSkillPart, int>>(() => new Dictionary<IArmorSetSkillPart, int>(SolverUtils.ArmorSetSkillPartEqualityComparer.Default));

        private void ReturnSlotsArray(int[] slotsArray)
        {
            for (int i = 0; i < slotsArray.Length; i++)
                slotsArray[i] = 0;

            availableSlotsObjectPool.PutObject(slotsArray);
        }

        private SolverDataJewelModel[] matchingJewels;
        private IAbility[] desiredAbilities;

        protected override void OnSearchBegin(SolverDataJewelModel[] matchingJewels, IAbility[] desiredAbilities)
        {
            this.matchingJewels = matchingJewels;
            this.desiredAbilities = desiredAbilities;
        }

        protected override ArmorSetSearchResult IsArmorSetMatching(IEquipment weapon, IEquipment[] equipments)
        {
            List<ArmorSetJewelResult> requiredJewels = jewelResultObjectPool.GetObject();
            int[] availableSlots = availableSlotsObjectPool.GetObject();

            void OnArmorSetMismatch()
            {
                requiredJewels.Clear();
                jewelResultObjectPool.PutObject(requiredJewels);

                ReturnSlotsArray(availableSlots);
            }

            SolverUtils.AccumulateAvailableSlots(weapon, availableSlots);

            foreach (IEquipment equipment in equipments)
                SolverUtils.AccumulateAvailableSlots(equipment, availableSlots);

            foreach (IAbility ability in desiredAbilities)
            {
                int armorAbilityTotal = 0;

                if (IsAbilityMatchingArmorSet(ability, equipments))
                    continue;

                foreach (IEquipment equipment in equipments)
                {
                    if (equipment == null)
                        continue;

                    foreach (IAbility a in equipment.Abilities)
                    {
                        if (a.Skill.Id == ability.Skill.Id)
                            armorAbilityTotal += a.Level;
                    }
                }

                int remaingAbilityLevels = ability.Level - armorAbilityTotal;

                if (remaingAbilityLevels <= 0)
                    continue;

                if (SolverUtils.AreAllSlotsUsed(availableSlots))
                {
                    OnArmorSetMismatch();
                    return ArmorSetSearchResult.NoMatch;
                }

                foreach (SolverDataJewelModel j in matchingJewels)
                {
                    // bold assumption, will be fucked if they decide to create jewels with multiple skills
                    IAbility a = j.Jewel.Abilities[0];

                    if (a.Skill.Id != ability.Skill.Id)
                        continue;

                    int requiredJewelCount = remaingAbilityLevels / a.Level; // This will turn into a bug with jewels providing skill with level 2 or more.

                    if (j.Available < requiredJewelCount)
                    {
                        OnArmorSetMismatch();
                        return ArmorSetSearchResult.NoMatch;
                    }

                    if (SolverUtils.ConsumeSlots(availableSlots, j.Jewel.SlotSize, requiredJewelCount) != requiredJewelCount)
                    {
                        OnArmorSetMismatch();
                        return ArmorSetSearchResult.NoMatch;
                    }

                    remaingAbilityLevels -= requiredJewelCount * a.Level;

                    requiredJewels.Add(new ArmorSetJewelResult { Jewel = j.Jewel, Count = requiredJewelCount });

                    break;
                }

                if (remaingAbilityLevels > 0)
                {
                    OnArmorSetMismatch();
                    return ArmorSetSearchResult.NoMatch;
                }
            }

            return new ArmorSetSearchResult
            {
                IsMatch = true,
                Jewels = requiredJewels,
                SpareSlots = availableSlots
            };
        }

        private bool IsAbilityMatchingArmorSet(IAbility ability, IEquipment[] armorPieces)
        {
            Dictionary<IArmorSetSkillPart, int> armorSetSkillParts = armorSetSkillPartsObjectPool.GetObject();

            void Done()
            {
                armorSetSkillParts.Clear();
                armorSetSkillPartsObjectPool.PutObject(armorSetSkillParts);
            }

            foreach (IEquipment equipment in armorPieces)
            {
                var armorPiece = equipment as IArmorPiece;

                if (armorPiece == null)
                    continue;

                if (armorPiece.ArmorSetSkills == null)
                    continue;

                foreach (IArmorSetSkill armorSetSkill in armorPiece.ArmorSetSkills)
                {
                    foreach (IArmorSetSkillPart armorSetSkillPart in armorSetSkill.Parts)
                    {
                        foreach (IAbility a in armorSetSkillPart.GrantedSkills)
                        {
                            if (a.Skill.Id == ability.Skill.Id)
                            {
                                if (armorSetSkillParts.TryGetValue(armorSetSkillPart, out int value) == false)
                                    value = 0;

                                armorSetSkillParts[armorSetSkillPart] = value + 1;
                            }
                        }
                    }
                }
            }

            if (armorSetSkillParts.Count > 0)
            {
                foreach (KeyValuePair<IArmorSetSkillPart, int> armorSetSkillPartKeyValue in armorSetSkillParts)
                {
                    if (armorSetSkillPartKeyValue.Value >= armorSetSkillPartKeyValue.Key.RequiredArmorPieces)
                    {
                        foreach (IAbility x in armorSetSkillPartKeyValue.Key.GrantedSkills)
                        {
                            if (x.Skill.Id == ability.Skill.Id)
                            {
                                Done();
                                return true;
                            }
                        }
                    }
                }
            }

            Done();
            return false;
        }
    }
}
