using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;
using Newtonsoft.Json;

namespace MHArmory.MhwDbDataSource.DataStructures
{
    internal class ArmorSetBonusRankPrimitive
    {
        [JsonProperty("pieces")]
        public int PieceCount { get; set; }
        [JsonProperty("skill")]
        public ArmorAbilityPrimitive Skill { get; set; }
    }

    internal class ArmorSetBonusPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("ranks")]
        public ArmorSetBonusRankPrimitive[] Ranks { get; set; }
    }

    internal class ArmorPieceIdPrimitive
    {
        [JsonProperty("id")]
        public int ArmorPieceId { get; set; }
    }

    internal class ArmorSetPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("rank")]
        public string Rank { get; set; }
        [JsonProperty("pieces")]
        public ArmorPieceIdPrimitive[] ArmorPieces { get; set; }
        [JsonProperty("bonus")]
        public ArmorSetBonusPrimitive Bonus { get; set; }
    }

    internal class ArmorPieceDefensePrimitive : IArmorPieceDefense
    {
        [JsonProperty("base")]
        public int Base { get; set; }
        [JsonProperty("max")]
        public int Max { get; set; }
        [JsonProperty("augmented")]
        public int Augmented { get; set; }
    }

    internal class ArmorPieceResistancesPrimitive : IArmorPieceResistances
    {
        [JsonProperty("fire")]
        public int Fire { get; set; }
        [JsonProperty("water")]
        public int Water { get; set; }
        [JsonProperty("ice")]
        public int Thunder { get; set; }
        [JsonProperty("thunder")]
        public int Ice { get; set; }
        [JsonProperty("dragon")]
        public int Dragon { get; set; }
    }

    internal class ArmorPieceAttributesPrimitive
    {
        [JsonProperty("requiredGender")]
        public string RequiredGender { get; set; }
    }

    internal class ArmorPieceAttributes : IArmorPieceAttributes
    {
        public Gender RequiredGender { get; }

        public ArmorPieceAttributes(ArmorPieceAttributesPrimitive attributes)
        {
            RequiredGender = GenderFromString(attributes.RequiredGender);
        }

        private static Gender GenderFromString(string gender)
        {
            switch (gender)
            {
                case "male": return Gender.Male;
                case "female": return Gender.Female;
                case null: return Gender.Both;
            }

            return Gender.None;
        }

        public static string GenderToString(Gender gender)
        {
            switch (gender)
            {
                case Gender.Male: return "male";
                case Gender.Female: return "female";
            }

            return null;
        }
    }

    internal class ArmorPieceAssets : IArmorPieceAssets
    {
        [JsonProperty("imageMale")]
        public string ImageMale { get; set; }
        [JsonProperty("imageFemale")]
        public string ImageFemale { get; set; }
    }

    internal class ArmorAbilityPrimitive
    {
        [JsonProperty("skill")]
        public int SkillId { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
    }

    internal class ArmorSlotRank
    {
        [JsonProperty("rank")]
        public int Rank { get; set; }
    }

    internal class ArmorPiecePrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public EquipmentType Type { get; set; }
        [JsonProperty("rarity")]
        public int Rarity { get; set; }
        [JsonProperty("defense")]
        public ArmorPieceDefensePrimitive Defense { get; set; }
        [JsonProperty("resistances")]
        public ArmorPieceResistancesPrimitive Resistances { get; set; }
        [JsonProperty("attributes")]
        public ArmorPieceAttributesPrimitive Attributes { get; set; }
        [JsonProperty("slots")]
        public IList<ArmorSlotRank> Slots { get; set; }
        [JsonProperty("skills")]
        public IList<ArmorAbilityPrimitive> Abilities { get; set; }
        [JsonProperty("assets")]
        public ArmorPieceAssets Assets { get; set; }
    }

    internal class ArmorPiece : IArmorPiece
    {
        public int Id { get; }
        public string Name { get; }
        public EquipmentType Type { get; }
        public int Rarity { get; }
        public IArmorPieceDefense Defense { get; }
        public IArmorPieceResistances Resistances { get; }
        public IArmorPieceAttributes Attributes { get; }
        public int[] Slots { get; }
        public IAbility[] Abilities { get; }
        public IArmorSetSkill[] ArmorSetSkills { get; }
        public IArmorPieceAssets Assets { get; }
        public IArmorSet ArmorSet { get; private set; }
        public IEvent Event { get; }

        public ArmorPiece(ArmorPiecePrimitive primitive, IAbility[] abilities)
        {
            Id = primitive.Id;
            Name = FixName(MapToGameName(primitive.Name));
            Type = primitive.Type;
            Rarity = primitive.Rarity;
            Defense = primitive.Defense;
            Resistances = primitive.Resistances;
            Attributes = new ArmorPieceAttributes(primitive.Attributes);
            Slots = primitive.Slots.Select(x => x.Rank).ToArray();
            Abilities = abilities;
            Assets = primitive.Assets;
            ArmorSet = null;
        }

        internal void UpdateArmorSet(IArmorSet armorSet)
        {
            ArmorSet = armorSet;
        }

        private string MapToGameName(string name)
        {
            if (name.EndsWith(" Alpha"))
                return name.Substring(0, name.Length - 5) + "α";

            if (name.EndsWith(" Beta"))
                return name.Substring(0, name.Length - 4) + "β";

            if (name.EndsWith(" Gamma"))
                return name.Substring(0, name.Length - 5) + "γ";

            return name;
        }

        private string FixName(string name)
        {
            if (name == "Death Stench Heels")
                return "Death Stench Heel";
            if (name == "Kulu-Yaku Head α")
                return "Kulu-Ya-Ku Head α";

            return name;
        }

        public override string ToString()
        {
            return $"{Name} ({Abilities.Length} abilities)";
        }
    }
}
