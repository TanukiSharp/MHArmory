using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search
{
    public static class DataUtility
    {
        public static bool AreOnSameFullArmorSet(IEnumerable<IEquipment> armorPieces)
        {
            int armorSetId = -1;

            foreach (IEquipment equipment in armorPieces)
            {
                var armorPiece = equipment as IArmorPiece;

                if (armorPiece == null)
                    continue;

                if (armorPiece.FullArmorSet == null)
                    return false;

                if (armorSetId < 0)
                    armorSetId = armorPiece.FullArmorSet.Id;
                else if (armorSetId != armorPiece.FullArmorSet.Id)
                    return false;
            }

            return true;
        }

        public static bool AreAbilitiesOnSameSkill(IAbility ability1, IAbility ability2)
        {
            return ability1 != null && ability2 != null && ability1.Skill.Id == ability2.Skill.Id;
        }

        public static int SlotSizeScoreSquare(int[] slots)
        {
            int total = 0;

            for (int i = 0; i < slots.Length; i++)
            {
                int slotSize = i + 1;
                int weight = slotSize * slotSize; // square
                total += slots[i] * weight;
            }

            return total;
        }

        public static int SlotSizeScoreCube(int[] slots)
        {
            int total = 0;

            for (int i = 0; i < slots.Length; i++)
            {
                int slotSize = i + 1;
                int weight = slotSize * slotSize * slotSize; // cube
                total += slots[i] * weight;
            }

            return total;
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
