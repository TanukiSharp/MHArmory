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
        public static bool AreOnSameFullArmorSet(IEnumerable<IArmorPiece> armorPieces)
        {
            int armorSetId = -1;

            foreach (IArmorPiece armorPiece in armorPieces)
            {
                if (armorPiece.ArmorSet == null || armorPiece.ArmorSet.IsFull == false)
                    return false;

                if (armorSetId < 0)
                    armorSetId = armorPiece.ArmorSet.Id;
                else if (armorSetId != armorPiece.ArmorSet.Id)
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
            return slots.Sum(x => x * x);
        }

        public static int SlotSizeScoreCube(int[] slots)
        {
            return slots.Sum(x => x * x * x);
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
