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
        IArmorPieceDefense Defense { get; }
        IArmorPieceResistances Resistances { get; }
        IArmorPieceAttributes Attributes { get; }
        IArmorPieceAssets Assets { get; }
        IArmorSet ArmorSet { get; }
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
            IArmorPieceAssets assets,
            IArmorSet armorSet
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
            ArmorSet = armorSet;
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
        public IArmorSet ArmorSet { get; private set; }

        public void UpdateArmorSet(IArmorSet armorSet)
        {
            if (ArmorSet == null)
                ArmorSet = armorSet;
            else
                ArmorSet = ArmorSet.Merge(armorSet);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public interface IArmorSetSkill
    {
        int RequiredArmorPieces { get; }
        IAbility[] GrantedSkills { get; }
    }

    public class ArmorSetSkill : IArmorSetSkill
    {
        public ArmorSetSkill(int requiredArmorPieces, IAbility[] grantedSkills)
        {
            RequiredArmorPieces = requiredArmorPieces;
            GrantedSkills = grantedSkills;
        }

        public int RequiredArmorPieces { get; }
        public IAbility[] GrantedSkills { get; }
    }

    public interface IArmorSet
    {
        int Id { get; }
        bool IsFull { get; }
        IArmorPiece[] ArmorPieces { get; }
        IArmorSetSkill[] Skills { get; }
    }

    public static class ArmorSetExtensions
    {
        public static IArmorSet Merge(this IArmorSet armorSet1, IArmorSet armorSet2)
        {
            if (armorSet1 == null && armorSet2 == null)
                return null;

            if (armorSet1 == null)
                return armorSet2;
            if (armorSet2 == null)
                return armorSet1;

            return new ArmorSet(
                armorSet1.Id,
                armorSet1.IsFull || armorSet2.IsFull,
                armorSet1.ArmorPieces,
                armorSet1.Skills ?? armorSet2.Skills
            );
        }
    }

    public class ArmorSet : IArmorSet
    {
        public ArmorSet(int id, bool isFull, IArmorPiece[] armorPieces, IArmorSetSkill[] skills)
        {
            Id = id;
            IsFull = isFull;
            ArmorPieces = armorPieces;
            Skills = skills;
        }

        public int Id { get; }
        public bool IsFull { get; }
        public IArmorPiece[] ArmorPieces { get; }
        public IArmorSetSkill[] Skills { get; }
    }
}
