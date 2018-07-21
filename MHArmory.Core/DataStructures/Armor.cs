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

    public class ArmorPieceDefense : IArmorPieceDefense
    {
        public ArmorPieceDefense(int baseDef, int maxDef, int augmentedDef)
        {
            Base = baseDef;
            Max = maxDef;
            Augmented = augmentedDef;
        }

        public int Base { get; }
        public int Max { get; }
        public int Augmented { get; }
    }

    public interface IArmorPieceResistances
    {
        int Fire { get; }
        int Water { get; }
        int Thunder { get; }
        int Ice { get; }
        int Dragon { get; }
    }

    public class ArmorPieceResistances : IArmorPieceResistances
    {
        public ArmorPieceResistances(int fire, int water, int thunder, int ice, int dragon)
        {
            Fire = fire;
            Water = water;
            Thunder = thunder;
            Ice = ice;
            Dragon = dragon;
        }

        public int Fire { get; }
        public int Water { get; }
        public int Thunder { get; }
        public int Ice { get; }
        public int Dragon { get; }
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

    public class ArmorPieceAttributes : IArmorPieceAttributes
    {
        public ArmorPieceAttributes(Gender requiredGender)
        {
            RequiredGender = requiredGender;
        }

        public Gender RequiredGender { get; }
    }

    public interface IArmorPieceAssets
    {
        string ImageMale { get; }
        string ImageFemale { get; }
    }

    public class ArmorPieceAssets : IArmorPieceAssets
    {
        public static readonly ArmorPieceAssets Null = new ArmorPieceAssets(null, null);

        public ArmorPieceAssets(string imageMale, string imageFemale)
        {
            ImageMale = imageMale;
            ImageFemale = imageFemale;
        }

        public string ImageMale { get; }
        public string ImageFemale { get; }
    }

    public interface IArmorPiece : IEquipment
    {
        int Id { get; }
        IArmorPieceDefense Defense { get; }
        IArmorPieceResistances Resistances { get; }
        IArmorPieceAttributes Attributes { get; }
        IArmorPieceAssets Assets { get; }
    }

    public class ArmorPiece : IArmorPiece
    {
        public ArmorPiece(
            int id,
            string name,
            EquipmentType equipmentType,
            int rarity,
            int[] slots,
            IAbility[] abilities,
            IArmorPieceDefense defense,
            IArmorPieceResistances resistances,
            IArmorPieceAttributes attributes,
            IArmorPieceAssets assets
        )
        {
            Id = id;
            Name = name;
            Type = equipmentType;
            Rarity = rarity;
            Slots = slots;
            Abilities = abilities;
            Defense = defense;
            Resistances = resistances;
            Attributes = attributes;
            Assets = assets;
        }

        public int Id { get; }
        public EquipmentType Type { get; }
        public string Name { get; }
        public int Rarity { get; }
        public int[] Slots { get; }
        public IAbility[] Abilities { get; }
        public IArmorPieceDefense Defense { get; }
        public IArmorPieceResistances Resistances { get; }
        public IArmorPieceAttributes Attributes { get; }
        public IArmorPieceAssets Assets { get; }
    }
}
