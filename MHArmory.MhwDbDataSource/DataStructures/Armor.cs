using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MHArmory.Core.DataStructures;
using Newtonsoft.Json;

namespace MHArmory.MhwDbDataSource.DataStructures
{
    internal class ArmorPieceDefense : IArmorPieceDefense
    {
        [JsonProperty("base")]
        public int Base { get; set; }
        [JsonProperty("max")]
        public int Max { get; set; }
        [JsonProperty("augmented")]
        public int Augmented { get; set; }
    }

    internal class ArmorPieceResistances : IArmorPieceResistances
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
        [JsonProperty("id")]
        public int AbilityId { get; set; }
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
        public ArmorPieceDefense Defense { get; set; }
        [JsonProperty("resistances")]
        public ArmorPieceResistances Resistances { get; set; }
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
        public IArmorPieceAssets Assets { get; }
        public IArmorSet ArmorSet { get; }

        public ArmorPiece(ArmorPiecePrimitive primitive, IAbility[] abilities)
        {
            Id = primitive.Id;
            Name = primitive.Name;
            Type = primitive.Type;
            Rarity = primitive.Rarity;
            Defense = primitive.Defense;
            Resistances = primitive.Resistances;
            Attributes = new ArmorPieceAttributes(primitive.Attributes);
            Slots = primitive.Slots.Select(x => x.Rank).ToArray();
            Abilities = primitive.Abilities.Select(x => abilities.FirstOrDefault(a => a.Id == x.AbilityId)).ToArray();
            Assets = primitive.Assets;
            ArmorSet = null; // TODO: update armor set
        }
    }
}
