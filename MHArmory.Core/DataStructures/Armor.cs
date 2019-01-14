using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    // Explicit enumeration because of OpenCL serialization, these values shouldn't change.
    public enum EquipmentType
    {
        Weapon = 0,
        Head = 1,
        Chest = 2,
        Gloves = 3,
        Waist = 4,
        Legs = 5,
        Charm = 6
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
        IArmorSetSkill[] ArmorSetSkills { get; }
        IFullArmorSet FullArmorSet { get; }
    }

    public class ArmorPiece : IArmorPiece
    {
        public ArmorPiece(
            int id,
            Dictionary<string, string> name,
            EquipmentType equipmentType,
            int rarity,
            int[] slots,
            IAbility[] abilities,
            IArmorSetSkill[] armorSetSkills,
            IArmorPieceDefense defense,
            IArmorPieceResistances resistances,
            IArmorPieceAttributes attributes,
            IArmorPieceAssets assets,
            IFullArmorSet fullArmorSet,
            IEvent evt
        )
        {
            Id = id;
            Name = name;
            Type = equipmentType;
            Rarity = rarity;
            Slots = slots;
            Abilities = abilities;
            ArmorSetSkills = armorSetSkills;
            Defense = defense;
            Resistances = resistances;
            Attributes = attributes;
            Assets = assets;
            FullArmorSet = fullArmorSet;
            Event = evt;
        }

        public int Id { get; }
        public EquipmentType Type { get; }
        public Dictionary<string, string> Name { get; }
        public int Rarity { get; }
        public int[] Slots { get; }
        public IAbility[] Abilities { get; }
        public IArmorSetSkill[] ArmorSetSkills { get; }
        public IArmorPieceDefense Defense { get; }
        public IArmorPieceResistances Resistances { get; }
        public IArmorPieceAttributes Attributes { get; }
        public IArmorPieceAssets Assets { get; }
        public IFullArmorSet FullArmorSet { get; private set; }
        public IEvent Event { get; }

        public void SetFullArmorSet(IFullArmorSet fullArmorSet)
        {
            FullArmorSet = fullArmorSet;
        }

        public override string ToString()
        {
            return Localization.GetDefault(Name);
        }
    }

    public interface IArmorSetSkillPart
    {
        int Id { get; }
        int RequiredArmorPieces { get; }
        IAbility[] GrantedSkills { get; }
    }

    public interface IArmorSetSkill
    {
        int Id { get; }
        Dictionary<string, string> Name { get; }
        IArmorSetSkillPart[] Parts { get; }
    }

    public class ArmorSetSkillPart : IArmorSetSkillPart
    {
        public ArmorSetSkillPart(int id, int requiredArmorPieces, IAbility[] grantedSkills)
        {
            Id = id;
            RequiredArmorPieces = requiredArmorPieces;
            GrantedSkills = grantedSkills;
        }

        public int Id { get; }
        public int RequiredArmorPieces { get; }
        public IAbility[] GrantedSkills { get; }
    }

    public class ArmorSetSkill : IArmorSetSkill
    {
        public ArmorSetSkill(int id, Dictionary<string, string> name, IArmorSetSkillPart[] parts)
        {
            Id = id;
            Name = name;
            Parts = parts;
        }

        public int Id { get; }
        public Dictionary<string, string> Name { get; }
        public IArmorSetSkillPart[] Parts { get; }
    }

    public interface IFullArmorSet
    {
        int Id { get; }
        IArmorPiece[] ArmorPieces { get; }
    }

    public class FullArmorSet : IFullArmorSet
    {
        public FullArmorSet(int id, IArmorPiece[] armorPieces)
        {
            if (armorPieces.Length != 5)
                throw new ArgumentException($"Invalid armor pieces count: {armorPieces.Length}");

            Id = id;
            ArmorPieces = armorPieces;
        }

        public int Id { get; }
        public IArmorPiece[] ArmorPieces { get; }
    }
}
