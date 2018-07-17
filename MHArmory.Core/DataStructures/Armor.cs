using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public enum EquipmentType
    {
        Weapon,
        Head,
        Chest,
        Gloves,
        Waist,
        Legs,
        Charm
    }

    public interface IArmorPieceDefense
    {
        int Base { get; }
        int Max { get; }
        int Augmented { get; }
    }

    public interface IArmorPieceResistances
    {
        int Fire { get; }
        int Water { get; }
        int Thunder { get; }
        int Ice { get; }
        int Dragon { get; }
    }

    public enum Gender
    {
        None,
        Male,
        Female,
        Both,
    }

    public interface IArmorPieceAttributes
    {
        Gender RequiredGender { get; }
    }

    public interface IArmorPieceAssets
    {
        string ImageMale { get; }
        string ImageFemale { get; }
    }

    public interface IArmorPiece : IEquipment
    {
        int Id { get; }
        IArmorPieceDefense Defense { get; }
        IArmorPieceResistances Resistances { get; }
        IArmorPieceAttributes Attributes { get; }
        IArmorPieceAssets Assets { get; }
    }

    public interface IEquipment
    {
        EquipmentType Type { get; }
        string Name { get; }
        int Rarity { get; }
        int[] Slots { get; }
        IAbility[] Abilities { get; }
    }
}
