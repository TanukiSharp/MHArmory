using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using Microsoft.Extensions.Logging;

namespace DataSourceTool
{
    public class Comparer
    {
        private ILogger logger;

        public async Task Run(string[] args)
        {
            logger = new ConsoleLogger();

            IDataSource source1 = new MHArmory.AthenaAssDataSource.DataSource(logger, null, null);
            IDataSource source2 = new MHArmory.ArmoryDataSource.DataSource();

            await Equals(source1, source2);
        }

        private async Task<bool> Equals(IDataSource x, IDataSource y)
        {
            if (x == null || y == null)
                return false;

            IArmorPiece[] armorPieces1 = await x.GetArmorPieces();
            ICharm[] charms1 = await x.GetCharms();
            IJewel[] jewels1 = await x.GetJewels();
            ISkill[] skills1 = await x.GetSkills();

            IArmorPiece[] armorPieces2 = await y.GetArmorPieces();
            ICharm[] charms2 = await y.GetCharms();
            IJewel[] jewels2 = await y.GetJewels();
            ISkill[] skills2 = await y.GetSkills();

            CheckAndReport(armorPieces1, armorPieces2, ArmorPieceEqualityComparer.Default);
            CheckAndReport(charms1, charms2, CharmEqualityComparer.Default);
            CheckAndReport(jewels1, jewels2, JewelEqualityComparer.Default);
            CheckAndReport(skills1, skills2, SkillEqualityComparer.Default);

            return true;
        }

        private bool CheckAndReport<T>(IEnumerable<T> source1, IEnumerable<T> source2, IEqualityComparer<T> equalityComparer)
        {
            IList<T> missing = source1
                .Except(source2, equalityComparer)
                .ToList();

            IList<T> surplus = source2
                .Except(source1, equalityComparer)
                .ToList();

            if (missing.Count > 0)
                logger.LogError($"{missing.Count} {typeof(T).Name} {(missing.Count > 1 ? "are" : "is")} missing.");

            if (surplus.Count > 0)
                logger.LogError($"{missing.Count} {typeof(T).Name} {(missing.Count > 1 ? "are" : "is")} in surplus.");

            if (missing.Count == 0 && surplus.Count == 0)
            {
                logger.LogInformation($"{typeof(T).Name} match.");
                return true;
            }

            return false;
        }

        // --------------------------------------------------------------------------------------------

        public class ArmorPieceEqualityComparer : EqualsBasedEqualityComparer<IArmorPiece>
        {
            public static readonly IEqualityComparer<IArmorPiece> Default = new ArmorPieceEqualityComparer();

            public override bool Equals(IArmorPiece x, IArmorPiece y)
            {
                return Comparer.Equals(x, y);
            }
        }

        public class CharmEqualityComparer : EqualsBasedEqualityComparer<ICharm>
        {
            public static readonly IEqualityComparer<ICharm> Default = new CharmEqualityComparer();

            public override bool Equals(ICharm x, ICharm y)
            {
                return Comparer.Equals(x, y);
            }
        }

        public class JewelEqualityComparer : EqualsBasedEqualityComparer<IJewel>
        {
            public static readonly IEqualityComparer<IJewel> Default = new JewelEqualityComparer();

            public override bool Equals(IJewel x, IJewel y)
            {
                return Comparer.Equals(x, y);
            }
        }

        public class SkillEqualityComparer : EqualsBasedEqualityComparer<ISkill>
        {
            public static readonly IEqualityComparer<ISkill> Default = new SkillEqualityComparer();

            public override bool Equals(ISkill x, ISkill y)
            {
                return Comparer.Equals(x, y);
            }
        }

        // --------------------------------------------------------------------------------------------

        private static bool SlotsEquals(int[] x, int[] y)
        {
            if (x == null || y == null)
                return x == y;

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }

        private static bool Equals(IEvent x, IEvent y)
        {
            if (x == null || y == null)
                return x == y;

            if (x.Id != y.Id)
                return false;

            if (x.Name != y.Name)
                return false;

            return true;
        }

        private static bool Equals(IAbility x, IAbility y)
        {
            if (x == null || y == null)
                return false;

            if (x.Id != y.Id)
                return false;

            if (x.Description != y.Description)
                return false;

            if (x.Skill.Id != y.Skill.Id)
                return false;

            if (x.Level != y.Level)
                return false;

            return true;
        }

        private static bool Equals(IArmorPieceDefense x, IArmorPieceDefense y)
        {
            if (x == null || y == null)
                return false;

            if (x.Base != y.Base)
                return false;

            if (x.Max != y.Max)
                return false;

            if (x.Augmented != y.Augmented)
                return false;

            return true;
        }

        private static bool Equals(IArmorPieceResistances x, IArmorPieceResistances y)
        {
            if (x == null || y == null)
                return false;

            if (x.Fire != y.Fire)
                return false;

            if (x.Water != y.Water)
                return false;

            if (x.Thunder != y.Thunder)
                return false;

            if (x.Ice != y.Ice)
                return false;

            if (x.Dragon != y.Dragon)
                return false;

            return true;
        }

        private static bool Equals(IArmorPieceAttributes x, IArmorPieceAttributes y)
        {
            if (x == null || y == null)
                return false;

            if (x.RequiredGender != y.RequiredGender)
                return false;

            return true;
        }

        private static bool Equals(IArmorSetSkillPart x, IArmorSetSkillPart y)
        {
            if (x == null || y == null)
                return false;

            if (x.Id != y.Id)
                return false;

            if (x.RequiredArmorPieces != y.RequiredArmorPieces)
                return false;

            if (x.GrantedSkills.Length != y.GrantedSkills.Length)
                return false;

            for (int i = 0; i < x.GrantedSkills.Length; i++)
            {
                if (Equals(x.GrantedSkills[i], y.GrantedSkills[i]) == false)
                    return false;
            }

            return true;
        }

        private static bool Equals(IArmorSetSkill x, IArmorSetSkill y)
        {
            if (x == null || y == null)
                return x == y;

            if (x.Id != y.Id)
                return false;

            if (x.Name != y.Name)
                return false;

            if (x.Parts.Length != y.Parts.Length)
                return false;

            for (int i = 0; i < x.Parts.Length; i++)
            {
                if (Equals(x.Parts[i], y.Parts[i]) == false)
                    return false;
            }

            return true;
        }

        private static bool Equals(IFullArmorSet x, IFullArmorSet y)
        {
            if (x == null || y == null)
                return x == y;

            if (x.Id != y.Id)
                return false;

            if (x.ArmorPieces.Length != y.ArmorPieces.Length)
                return false;

            for (int i = 0; i < x.ArmorPieces.Length; i++)
            {
                if (x.ArmorPieces[i].Id != y.ArmorPieces[i].Id)
                    return false;
            }

            return true;
        }

        private static bool Equals(IArmorPiece x, IArmorPiece y)
        {
            /*
                int Id { get; }
                EquipmentType Type { get; }
                string Name { get; }
                int Rarity { get; }
                int[] Slots { get; }
                IEvent Event { get; }
                IAbility[] Abilities { get; }
                IArmorPieceDefense Defense { get; }
                IArmorPieceResistances Resistances { get; }
                IArmorPieceAttributes Attributes { get; }
                IArmorPieceAssets Assets { get; }
                IArmorSetSkill[] ArmorSetSkills { get; }
                IFullArmorSet FullArmorSet { get; }
            */

            if (x == null || y == null)
                return false;

            if (x.Id != y.Id)
                return false;

            if (x.Type != y.Type)
                return false;

            if (x.Name != y.Name)
                return false;

            if (x.Rarity != y.Rarity)
                return false;

            if (x.Type != y.Type)
                return false;

            if (SlotsEquals(x.Slots, y.Slots) == false)
                return false;

            if (Equals(x.Event, y.Event) == false)
                return false;

            if (x.Abilities.Length != y.Abilities.Length)
                return false;

            for (int i = 0; i < x.Abilities.Length; i++)
            {
                if (Equals(x.Abilities[i], y.Abilities[i]) == false)
                    return false;
            }

            if (Equals(x.Defense, y.Defense) == false)
                return false;

            if (Equals(x.Resistances, y.Resistances) == false)
                return false;

            if (Equals(x.Attributes, y.Attributes) == false)
                return false;

            return true;
        }

        private static bool Equals(ICharmLevel x, ICharmLevel y)
        {
            /*
                int Id { get; }
                EquipmentType Type { get; }
                string Name { get; }
                int Rarity { get; }
                int[] Slots { get; }
                IEvent Event { get; }
                ICharm Charm { get; }
                int Level { get; }
                IAbility[] Abilities { get; }
            */

            if (x == null || y == null)
                return false;

            if (x.Id != y.Id)
                return false;

            if (x.Name != y.Name)
                return false;

            if (x.Type != y.Type)
                return false;

            if (x.Rarity != y.Rarity)
                return false;

            if (SlotsEquals(x.Slots, y.Slots) == false)
                return false;

            if (Equals(x.Event, y.Event) == false)
                return false;

            if (x.Charm.Id != y.Charm.Id)
                return false;

            if (x.Level != y.Level)
                return false;

            if (x.Abilities.Length != y.Abilities.Length)
                return false;

            for (int i = 0; i < x.Abilities.Length; i++)
            {
                if (Equals(x.Abilities[i], y.Abilities[i]) == false)
                    return false;
            }

            return true;
        }

        private static bool Equals(ICharm x, ICharm y)
        {
            if (x == null || y == null)
                return false;

            if (x.Id != y.Id)
                return false;

            if (x.Name != y.Name)
                return false;

            if (x.Levels.Length != y.Levels.Length)
                return false;

            for (int i = 0; i < x.Levels.Length; i++)
            {
                if (Equals(x.Levels[i], y.Levels[i]) == false)
                    return false;
            }

            return true;
        }

        private static bool Equals(IJewel x, IJewel y)
        {
            if (x == null || y == null)
                return false;

            if (x.Id != y.Id)
                return false;

            if (x.Name != y.Name)
                return false;

            if (x.Rarity != y.Rarity)
                return false;

            if (x.SlotSize != y.SlotSize)
                return false;

            if (x.Abilities.Length != y.Abilities.Length)
                return false;

            for (int i = 0; i < x.Abilities.Length; i++)
            {
                if (Equals(x.Abilities[i], y.Abilities[i]) == false)
                    return false;
            }

            return true;
        }

        private static bool Equals(ISkill x, ISkill y)
        {
            if (x == null || y == null)
                return false;

            if (x.Id != y.Id)
                return false;

            if (x.Name != y.Name)
                return false;

            if (x.MaxLevel != y.MaxLevel)
                return false;

            if (x.Description != y.Description)
                return false;

            if (x.Abilities.Length != y.Abilities.Length)
                return false;

            for (int i = 0; i < x.Abilities.Length; i++)
            {
                if (Equals(x.Abilities[i], y.Abilities[i]) == false)
                    return false;
            }

            return true;
        }

        // =================================================================================

        public abstract class EqualsBasedEqualityComparer<T> : IEqualityComparer<T>
        {
            protected EqualsBasedEqualityComparer()
            {
            }

            public abstract bool Equals(T x, T y);

            public int GetHashCode(T obj)
            {
                return 0;
            }
        }
    }
}
