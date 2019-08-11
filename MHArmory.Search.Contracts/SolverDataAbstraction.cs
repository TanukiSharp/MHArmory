using System;
using System.Collections.Generic;
using System.Text;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Contracts
{
    public interface ISolverDataEquipmentModel
    {
        IEquipment Equipment { get; }
        bool IsSelected { get; set; }
    }

    public interface ISolverData : IExtension
    {
        IEquipment Weapon { get; }
        ISolverDataEquipmentModel[] AllHeads { get; }
        ISolverDataEquipmentModel[] AllChests { get; }
        ISolverDataEquipmentModel[] AllGloves { get; }
        ISolverDataEquipmentModel[] AllWaists { get; }
        ISolverDataEquipmentModel[] AllLegs { get; }
        ISolverDataEquipmentModel[] AllCharms { get; }
        SolverDataJewelModel[] AllJewels { get; }
        IAbility[] DesiredAbilities { get; }

        void Setup(
            IEquipment weapon,
            IEnumerable<IArmorPiece> heads,
            IEnumerable<IArmorPiece> chests,
            IEnumerable<IArmorPiece> gloves,
            IEnumerable<IArmorPiece> waists,
            IEnumerable<IArmorPiece> legs,
            IEnumerable<ICharmLevel> charms,
            IEnumerable<SolverDataJewelModel> jewels,
            IEnumerable<IAbility> desiredAbilities
        );
    }
}
