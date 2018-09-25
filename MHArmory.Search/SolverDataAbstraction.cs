using System;
using System.Collections.Generic;
using System.Text;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search
{
    public interface ISolverDataEquipmentModel
    {
        IEquipment Equipment { get; }
        event EventHandler SelectionChanged;
        bool IsSelected { get; set; }
    }

    public interface ISolverData
    {
        int[] WeaponSlots { get; }
        int MinJewelSize { get; }
        int MaxJewelSize { get; }

        ISolverDataEquipmentModel[] AllHeads { get; }
        ISolverDataEquipmentModel[] AllChests { get; }
        ISolverDataEquipmentModel[] AllGloves { get; }
        ISolverDataEquipmentModel[] AllWaists { get; }
        ISolverDataEquipmentModel[] AllLegs { get; }
        ISolverDataEquipmentModel[] AllCharms { get; }
        SolverDataJewelModel[] AllJewels { get; }
        IAbility[] DesiredAbilities { get; }

        ISolverData Done();
    }
}
