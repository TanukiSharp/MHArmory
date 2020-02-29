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

        public static int ConsumeSlots(int[] availableSlots, int jewelSize, int jewelCount, bool limitToExactSlotsize = false)
        {
            int slotted = 0;
            for (int i = jewelSize - 1; i < availableSlots.Length; i++)
            {
                if (availableSlots[i] <= 0)
                {
                    if (limitToExactSlotsize)
                        return slotted;
                    else
                        continue;
                }

                if (availableSlots[i] >= jewelCount)
                {
                    availableSlots[i] -= jewelCount;
                    return jewelCount;
                }
                else
                {
                    jewelCount -= availableSlots[i];
                    slotted += availableSlots[i];
                    availableSlots[i] = 0;
                }
            }

            return slotted;
        }
    }
}
