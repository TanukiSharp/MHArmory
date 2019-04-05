using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.Core
{
    public class NullArmorPiece : IArmorPiece
    {
        public static readonly IArmorPiece Head = new NullArmorPiece(EquipmentType.Head);
        public static readonly IArmorPiece Chest = new NullArmorPiece(EquipmentType.Chest);
        public static readonly IArmorPiece Gloves = new NullArmorPiece(EquipmentType.Gloves);
        public static readonly IArmorPiece Waist = new NullArmorPiece(EquipmentType.Waist);
        public static readonly IArmorPiece Legs = new NullArmorPiece(EquipmentType.Legs);

        public IArmorPieceDefense Defense { get; } = new ArmorPieceDefense(0, 0, 0);
        public IArmorPieceResistances Resistances { get; } = new ArmorPieceResistances(0, 0, 0, 0, 0);
        public IArmorPieceAttributes Attributes { get; } = new ArmorPieceAttributes(Gender.Both);
        public IArmorPieceAssets Assets { get; } = new ArmorPieceAssets(null, null);
        public IArmorSetSkill[] ArmorSetSkills { get; } = new IArmorSetSkill[0];
        public IFullArmorSet FullArmorSet { get; } = null;
        public int Id { get; } = -1;
        public EquipmentType Type { get; }
        public Dictionary<string, string> Name { get; } = new Dictionary<string, string> { [Localization.DefaultLanguage] = "<none>" };
        public int Rarity { get; } = 0;
        public int[] Slots { get; } = new int[0];
        public IEvent Event { get; } = null;
        public IAbility[] Abilities { get; } = new IAbility[0];
        public ICraftMaterial[] CraftMaterials { get; } = new ICraftMaterial[0];

        public NullArmorPiece(EquipmentType type)
        {
            Type = type;
        }
    }
}
