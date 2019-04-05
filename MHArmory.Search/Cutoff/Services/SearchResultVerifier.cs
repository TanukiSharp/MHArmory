using System;
using System.Collections.Generic;
using System.Linq;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;
using MHArmory.Search.Cutoff.Models;
using MHArmory.Search.Cutoff.Models.Mapped;

namespace MHArmory.Search.Cutoff.Services
{
    internal class SearchResultVerifier : ISearchResultVerifier
    {
        public bool TryGetSearchResult(Combination combination, bool hasSuperset, out ArmorSetSearchResult result)
        {
            result = new ArmorSetSearchResult();

            bool skillSummingSuccess = SkillSummingCutoff(combination);
            if (!skillSummingSuccess)
            {
                return false;
            }

            result.Jewels = new List<ArmorSetJewelResult>();
            int[] totalSlots = (int[])combination.Slots.Clone();
            int[] remainingLevels = (int[])combination.RemainingSkills.Clone();

            foreach (MappedJewel mappedJewel in combination.Jewels)
            {
                MappedSkill ability = mappedJewel.Skill;
                int mappedId = ability.MappedId;
                int needLevels = remainingLevels[mappedId];
                if (needLevels <= 0)
                {
                    continue;
                }

                bool jewelSlottingSuccess = TrySlotJewels(mappedJewel, totalSlots, remainingLevels, out ArmorSetJewelResult jewelResult);
                if (!jewelSlottingSuccess)
                {
                    return false;
                }
                result.Jewels.Add(jewelResult);
            }

            if (hasSuperset)
            {
                bool skillDebtSuccess = SkillDebtCutoff(combination, totalSlots);
                if (!skillDebtSuccess)
                {
                    return false;
                }
            }

            bool success = remainingLevels.All(x => x <= 0);
            if (!success)
            {
                return false;
            }

            bool excludedCheckPass = CheckForExcludedSkills(combination);
            if (!excludedCheckPass)
            {
                return false;
            }

            result.SpareSlots = new int[CutoffSearchConstants.Slots];
            for (int i = 0; i < CutoffSearchConstants.Slots; i++)
            {
                result.SpareSlots[i] = totalSlots[i+1];
            }
            
            result.ArmorPieces = new List<IArmorPiece>(CutoffSearchConstants.ArmorTypes);
            for (int i = 0; i < CutoffSearchConstants.ArmorTypes; i++)
            {
                MappedEquipment equipment = combination.Equipments[i];
                if (equipment.Equipment != null)
                {
                    result.ArmorPieces.Add((IArmorPiece)equipment.Equipment);
                }
            }

            result.IsMatch = true;
            result.Charm = (ICharmLevel)combination.Equipments[CutoffSearchConstants.ArmorTypes].Equipment;
            return true;
        }

        private bool CheckForExcludedSkills(Combination combination)
        {
            foreach (int index in combination.ExcludedAbilityIndices)
            {
                if (combination.RemainingSkills[index] < 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool SkillDebtCutoff(Combination combination, int[] totalSlots)
        {
            int skillOverload = 0;
            for (int i = 0; i < combination.RemainingSkills.Length; i++)
            {
                int remainingSkill = combination.RemainingSkills[i];
                if (remainingSkill < 0)
                {
                    skillOverload -= remainingSkill;
                }
            }
            
            int debt = 0;
            for (int i = 0; i < CutoffSearchConstants.EquipmentTypes; i++)
            {
                debt += combination.Equipments[i].SkillDebt;
            }

            int remainingSlots = 0;
            for (int i = 1; i <= CutoffSearchConstants.Slots; ++i)
            {
                remainingSlots += totalSlots[i];
            }

            int sum = skillOverload + remainingSlots;
            if (sum < debt)
            {
                return false;
            }

            return true;
        }

        private bool TrySlotJewels(MappedJewel mappedJewel, int[] totalSlots, int[] remainingLevels, out ArmorSetJewelResult jewelResult)
        {
            jewelResult = new ArmorSetJewelResult();

            IJewel jewel = mappedJewel.Jewel.Jewel;
            MappedSkill ability = mappedJewel.Skill;
            int mappedId = ability.MappedId;
            int needLevels = remainingLevels[mappedId];
            int needJewels = needLevels;
            int canTakeJewels = Math.Min(mappedJewel.Jewel.Available, needJewels);
            if (canTakeJewels < needJewels)
            {
                return false;
            }

            int jewelsTaken = 0;
            for (int slot = jewel.SlotSize; slot <= CutoffSearchConstants.Slots && jewelsTaken < needJewels; ++slot)
            {
                int sizeSlotsAvailable = totalSlots[slot];
                int canFitToSlot = Math.Min(sizeSlotsAvailable, canTakeJewels);
                totalSlots[slot] -= canFitToSlot;
                canTakeJewels -= canFitToSlot;
                jewelsTaken += canFitToSlot;
            }

            if (jewelsTaken < needJewels)
            {
                return false;
            }

            remainingLevels[mappedId] -= jewelsTaken;

            jewelResult.Jewel = jewel;
            jewelResult.Count = jewelsTaken;
            return true;
        }

        private bool SkillSummingCutoff(Combination combination)
        {
            int sum = 0;
            foreach (int remainingSkill in combination.RemainingSkills)
            {
                if (remainingSkill > 0)
                {
                    sum += remainingSkill;
                }
            }

            for (int i = 1; i <= CutoffSearchConstants.Slots; i++)
            {
                sum -= combination.Slots[i];
            }
            return sum <= 0;
        }
    }
}
