using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search
{
    public static class DataUtility
    {
        public static bool AreAbilitiesOnSameSkill(IAbility ability1, IAbility ability2)
        {
            return ability1 != null && ability2 != null && ability1.Skill.Id == ability2.Skill.Id;
        }

        public static int SlotSizeScoreSquare(int[] slots)
        {
            return slots.Sum(x => x * x);
        }

        public static int SlotSizeScoreCube(int[] slots)
        {
            return slots.Sum(x => x * x * x);
        }

        public static IEnumerable<T> CreateArmorPieceSorter<T>(IEnumerable<T> items, IEnumerable<MaximizedSearchCriteria> sortCriterias) where T : ISolverDataEquipmentModel
        {
            if (items.Any() == false)
                return items;

            IOrderedEnumerable<T> result = items.OrderBy(x => 1); // wasting a bit of CPU cycles for productivity purpose :(

            foreach (MaximizedSearchCriteria sortCriteria in sortCriterias)
            {
                switch (sortCriteria)
                {
                    case MaximizedSearchCriteria.BaseDefense:
                        result = result.ThenByDescending(x => ((IArmorPiece)x.Equipment).Defense.Base);
                        break;
                    case MaximizedSearchCriteria.MaxUnaugmentedDefense:
                        result = result.ThenByDescending(x => ((IArmorPiece)x.Equipment).Defense.Max);
                        break;
                    case MaximizedSearchCriteria.MaxAugmentedDefense:
                        result = result.ThenByDescending(x => ((IArmorPiece)x.Equipment).Defense.Augmented);
                        break;
                    case MaximizedSearchCriteria.Rarity:
                        result = result.ThenBy(x => x.Equipment.Rarity);
                        break;
                    case MaximizedSearchCriteria.SlotCount:
                        result = result
                            .ThenByDescending(x => x.Equipment.Slots.Count(s => s > 0))
                            .ThenByDescending(x => x.Equipment.Slots.Sum()); // For same amount of slots, set larger slots first.
                        break;
                    case MaximizedSearchCriteria.SlotSizeSquare:
                        result = result
                            .ThenByDescending(x => SlotSizeScoreSquare(x.Equipment.Slots))
                            .ThenByDescending(x => x.Equipment.Slots.Count(s => s > 0)); // For same score, gives precedence to slot count. (only 221 and 3--, sets 221 before 3--)
                        break;
                    case MaximizedSearchCriteria.SlotSizeCube:
                        result = result.ThenByDescending(x => SlotSizeScoreCube(x.Equipment.Slots));
                        break;
                    case MaximizedSearchCriteria.FireResistance:
                        result = result.ThenByDescending(x => ((IArmorPiece)x.Equipment).Resistances.Fire);
                        break;
                    case MaximizedSearchCriteria.WaterResistance:
                        result = result.ThenByDescending(x => ((IArmorPiece)x.Equipment).Resistances.Water);
                        break;
                    case MaximizedSearchCriteria.ThunderResistance:
                        result = result.ThenByDescending(x => ((IArmorPiece)x.Equipment).Resistances.Thunder);
                        break;
                    case MaximizedSearchCriteria.IceResistance:
                        result = result.ThenByDescending(x => ((IArmorPiece)x.Equipment).Resistances.Ice);
                        break;
                    case MaximizedSearchCriteria.DragonResistance:
                        result = result.ThenByDescending(x => ((IArmorPiece)x.Equipment).Resistances.Dragon);
                        break;
                }
            }

            return result;
        }

        public static bool IsLessPoweredEquivalentEquipment(IEquipment baseEquipment, IEquipment checkedEquipment)
        {
            if (checkedEquipment.Slots.Length > baseEquipment.Slots.Length)
                return false;

            for (int i = 0; i < checkedEquipment.Slots.Length; i++)
            {
                if (checkedEquipment.Slots[i] > baseEquipment.Slots[i])
                    return false;
            }

            if (checkedEquipment.Abilities.Length > baseEquipment.Abilities.Length)
                return false;

            IAbility[] checkedAbilities = checkedEquipment.Abilities;
            IAbility[] baseAbilities = baseEquipment.Abilities;

            for (int i = 0; i < checkedAbilities.Length; i++)
            {
                bool hasMatchingAbility = false;

                for (int j = 0; j < baseAbilities.Length; j++)
                {
                    if (checkedAbilities[i].Skill.Id == baseAbilities[j].Skill.Id)
                    {
                        if (checkedAbilities[i].Level > baseAbilities[j].Level)
                            return false;

                        hasMatchingAbility = true;
                    }
                }

                if (hasMatchingAbility == false)
                    return false;
            }

            return true;
        }

        public static bool IsLessPoweredEquivalentArmorPiece(IArmorPiece baseArmorPiece, IArmorPiece checkedArmorPiece)
        {
            if (IsLessPoweredEquivalentEquipment(baseArmorPiece, checkedArmorPiece) == false)
                return false;

            IAbility[] checkedAbilities = checkedArmorPiece.Abilities;
            IAbility[] baseAbilities = baseArmorPiece.Abilities;

            // From here, checkedAbilities has less or equal amount of abilities compared to baseAbilities.

            if (checkedAbilities.Length == baseAbilities.Length)
            {
                // When there is an equal amount of abilities, they match their skills.

                for (int i = 0; i < checkedAbilities.Length; i++)
                {
                    for (int j = 0; j < baseAbilities.Length; j++)
                    {
                        if (checkedAbilities[i].Skill.Id == baseAbilities[j].Skill.Id)
                        {
                            // The double loop with skill equality check is to avoid ordering issues.
                            // Ability checkedAbilities[i] is not necessarily the same as baseAbilities[i].

                            if (checkedAbilities[i].Level < baseAbilities[j].Level)
                                return true;
                        }
                    }
                }

                // Here all skills have equal level of ability, let's consider as less powered
                // if the checkedArmorPiece has less augmented defense.

                return checkedArmorPiece.Defense.Augmented <= baseArmorPiece.Defense.Augmented;
            }

            return true;
        }
    }
}
