using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Default
{
    public static class SolverUtils
    {
        public class ArmorSetSkillPartEqualityComparer : IEqualityComparer<IArmorSetSkillPart>
        {
            public static readonly IEqualityComparer<IArmorSetSkillPart> Default = new ArmorSetSkillPartEqualityComparer();

            public bool Equals(IArmorSetSkillPart x, IArmorSetSkillPart y)
            {
                if (x == null || y == null)
                    return false;

                return x.Id == y.Id;
            }

            public int GetHashCode(IArmorSetSkillPart obj)
            {
                if (obj == null)
                    return 0;

                return obj.Id;
            }
        }

        public static bool IsAnyFullArmorSet(IEquipment[] equipments)
        {
            foreach (IEquipment equipment in equipments)
            {
                if (equipment is IArmorPiece armorPiece && armorPiece.FullArmorSet != null)
                    return true;
            }

            return false;
        }

        public static bool AreAllSlotsUsed(int[] availableSlots)
        {
            foreach (int x in availableSlots)
            {
                if (x > 0)
                    return false;
            }

            return true;
        }

        public static void AccumulateAvailableSlots(IEquipment equipment, int[] availableSlots)
        {
            if (equipment == null)
                return;

            foreach (int slotSize in equipment.Slots)
            {
                if (slotSize > 0)
                    availableSlots[slotSize - 1]++;
            }
        }

        public static int ComputeRequiredJewelsCount(int remaingAbilityLevels, int skillLevel, bool allowWaste)
        {
            int count = remaingAbilityLevels / skillLevel;

            if (allowWaste && skillLevel * count < remaingAbilityLevels)
                count++;

            return count;
        }


        public static bool IsAbilityMatchingArmorSet(IAbility ability, IEquipment[] armorPieces, ObjectPool<Dictionary<IArmorSetSkillPart, int>> armorSetSkillPartsObjectPool)
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
