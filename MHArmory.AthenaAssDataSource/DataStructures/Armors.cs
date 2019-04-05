using System;
using System.Collections.Generic;
using System.Text;
using MHArmory.Core.DataStructures;

namespace MHArmory.AthenaAssDataSource.DataStructures
{
    internal class ArmorPieceContainer
    {
        internal ArmorPiecePrimitive Primitive;
        internal Dictionary<string, string> Names;
        internal EquipmentType Type;
        internal ArmorPiece ArmorPiece;
    }

    internal class ArmorPiecePrimitive
    {
        [Hidden]
        internal int Id;
        internal string Name = null;
        [Name("Restriction", "Gender")]
        internal string Restriction = null;
        internal int Rarity = 0;
        internal string Slots = null;
        [Name("Min Def")]
        internal int MinDef = 0;
        [Name("Max Def")]
        internal int MaxDef = 0;
        [Name("Aug Def")]
        internal int AugmentedDef = 0;
        [Name("Fire")]
        internal int FireRes = 0;
        [Name("Water")]
        internal int WaterRes = 0;
        [Name("Thunder")]
        internal int ThunderRes = 0;
        [Name("Ice")]
        internal int IceRes = 0;
        [Name("Dragon")]
        internal int DragonRes = 0;
        [Name("Skill 1")]
        internal string Skill1 = null;
        [Name("Points 1")]
        internal int PointSkill1 = 0;
        [Name("Skill 2")]
        internal string Skill2 = null;
        [Name("Points 2")]
        internal int PointSkill2 = 0;
        [Name("Skill 3")]
        internal string Skill3 = null;
        [Name("Points 3")]
        internal int PointSkill3 = 0;
        [Name("Material 1")]
        internal string Material1 = null;
        [Name("Num 1")]
        internal int Material1Points = 0;
        [Name("Material 2")]
        internal string Material2 = null;
        [Name("Num 2")]
        internal int Material2Points = 0;
        [Name("Material 3")]
        internal string Material3 = null;
        [Name("Num 3")]
        internal int Material3Points = 0;
        [Name("Material 4")]
        internal string Material4 = null;
        [Name("Num 4")]
        internal int Material4Points = 0;
    }
}
