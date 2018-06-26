using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public enum ArmorPieceType
    {
        Head,
        Body,
        Arms,
        Waist,
        Legs,
    }

    public interface ArmorPieceDefense
    {
        int Base { get; }
        int Max { get; }
        int Augmented { get; }
    }

    public interface ArmorPieceResistances
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

    public interface ArmorPieceAttributes
    {
        Gender RequiredGender { get; }
    }

    public interface ArmorPieceAssets
    {
        string ImageMale { get; }
        string ImageFemale { get; }
    }

    public interface IArmorPiece
    {
        int Id { get; }
        int Name { get; }
        ArmorPieceType Type { get; }
        int Rarity { get; }
        ArmorPieceDefense Defense { get; }
        ArmorPieceResistances Resistances { get; }
        int[] Slots { get; }
        ISkill[] Skills { get; }
        ArmorPieceAssets Assets { get; }
    }
}
