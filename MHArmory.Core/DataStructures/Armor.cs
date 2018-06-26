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

    public class ArmorPieceDefense
    {
        public int Base { get; }
        public int Max { get; }
        public int Augmented { get; }

        public ArmorPieceDefense(int baseDefense, int maxDefense, int augmentedDefense)
        {
            Base = baseDefense;
            Max = maxDefense;
            Augmented = augmentedDefense;
        }
    }

    public class ArmorPieceResistances
    {
        public int Fire { get; }
        public int Water { get; }
        public int Thunder { get; }
        public int Ice { get; }
        public int Dragon { get; }

        public ArmorPieceResistances(int fire = 0, int water = 0, int thunder = 0, int ice = 0, int dragon = 0)
        {
            Fire = fire;
            Water = water;
            Thunder = thunder;
            Ice = ice;
            Dragon = dragon;
        }
    }

    public enum Gender
    {
        None,
        Male,
        Female,
        Both,
    }

    public class ArmorPieceAttributes
    {
        public Gender RequiredGender { get; }

        public ArmorPieceAttributes(Gender requiredGender = Gender.None)
        {
            RequiredGender = requiredGender;
        }
    }

    public class ArmorPieceAssets
    {
        public string ImageMale { get; }
        public string ImageFemale { get; }

        public ArmorPieceAssets(string imageMale, string imageFemale)
        {
            ImageMale = imageMale;
            ImageFemale = imageFemale;
        }
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
