using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Default
{
    public static class SolverUtils
    {
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
    }
}
