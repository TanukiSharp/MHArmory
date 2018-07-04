using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.Core
{
    public interface IArmorDataSource
    {
        string Description { get; }
        Task<IArmorPiece[]> GetArmorPieces();
    }

    public interface ISkillDataSource
    {
        string Description { get; }
        Task<ISkill[]> GetSkills();
        Task<IAbility[]> GetAbilities();
    }

    public interface ICharmDataSource
    {
        string Description { get; }
        Task<ICharm[]> GetCharms();
    }

    public interface IJewelDataSource
    {
        string Description { get; }
        Task<IJewel[]> GetJewels();
    }

    public static class ArmorUtility
    {
        public static IList<IArmorPiece> ExcludeLessPoweredEquivalent(IList<IArmorPiece> armorPieces)
        {
            for (int i = 0; i < armorPieces.Count; i++)
            {
                for (int j = 0; j < armorPieces.Count; j++)
                {
                    if (i == j)
                        continue;

                    if (IsLessPoweredEquivalent(armorPieces[i], armorPieces[j]))
                    {
                        armorPieces.RemoveAt(j);

                        if (i >= j)
                            i--;
                        j--;
                    }
                }
            }

            return armorPieces;
        }

        public static bool IsLessPoweredEquivalent(IArmorPiece baseArmorPiece, IArmorPiece checkedArmorPiece)
        {
            if (checkedArmorPiece.Slots.Length > baseArmorPiece.Slots.Length)
                return false;

            for (int i = 0; i < checkedArmorPiece.Slots.Length; i++)
            {
                if (checkedArmorPiece.Slots[i] > baseArmorPiece.Slots[i])
                    return false;
            }

            if (checkedArmorPiece.Abilities.Length > baseArmorPiece.Abilities.Length)
                return false;

            IAbility[] checkedAbilities = checkedArmorPiece.Abilities;
            IAbility[] baseAbilities = baseArmorPiece.Abilities;

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

            if (checkedAbilities.Length == baseAbilities.Length)
            {
                for (int i = 0; i < checkedAbilities.Length; i++)
                {
                    if (checkedAbilities[i].Level != baseAbilities[i].Level)
                        return false;
                }

                return checkedArmorPiece.Defense.Augmented <= baseArmorPiece.Defense.Augmented;
            }

            return true;
        }
    }
}
