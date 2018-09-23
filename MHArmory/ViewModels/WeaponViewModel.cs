using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.ArmoryDataSource.DataStructures;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public enum WeaponDamageType
    {
        Severe,
        Blunt,
        Projectile
    }

    public enum WeaponElderseal
    {
        None,
        Low,
        Average,
        High
    }

    public enum WeaponDeviation
    {
        Unset,
        None,
        Low,
        Average,
        High
    }

    public class WeaponAttributesViewModel
    {
        public WeaponDamageType DamageType { get; }
        public WeaponElderseal Elderseal { get; }
        public int Affinity { get; }
        public int Defense { get; }
        public WeaponDeviation Deviation { get; }

        public WeaponAttributesViewModel(WeaponAttributesPrimitive primitive)
        {
            switch (primitive.DamageType)
            {
                case "severe":
                    DamageType = WeaponDamageType.Severe;
                    break;
                case "blunt":
                    DamageType = WeaponDamageType.Blunt;
                    break;
                case "projectile":
                    DamageType = WeaponDamageType.Projectile;
                    break;
            }

            switch (primitive.Elderseal)
            {
                case "low":
                    Elderseal = WeaponElderseal.Low;
                    break;
                case "average":
                    Elderseal = WeaponElderseal.Average;
                    break;
                case "high":
                    Elderseal = WeaponElderseal.High;
                    break;
                default:
                    Elderseal = WeaponElderseal.None;
                    break;
            }

            switch (primitive.Deviation)
            {
                case "none":
                    Deviation = WeaponDeviation.None;
                    break;
                case "low":
                    Deviation = WeaponDeviation.Low;
                    break;
                case "average":
                    Deviation = WeaponDeviation.Average;
                    break;
                case "high":
                    Deviation = WeaponDeviation.High;
                    break;
                default:
                    Deviation = WeaponDeviation.Unset;
                    break;
            }

            Affinity = primitive.Affinity;
            Defense = primitive.Defense;
        }
    }

    public class WeaponElementViewModel
    {
        public ElementType Type { get; }
        public int Value { get; }
        public bool IsHidden { get; }

        public WeaponElementViewModel(WeaponElementPrimitive primitive)
        {
            Type = ConvertElementType(primitive.Type);
            Value = primitive.Value;
            IsHidden = primitive.IsHidden;
        }

        private ElementType ConvertElementType(string elementType)
        {
            switch (elementType)
            {
                case "fire": return ElementType.Fire;
                case "water": return ElementType.Water;
                case "thunder": return ElementType.Thunder;
                case "ice": return ElementType.Ice;
                case "dragon": return ElementType.Dragon;
                case "poison": return ElementType.Poison;
                case "sleep": return ElementType.Sleep;
                case "paralysis": return ElementType.Paralysis;
                case "blast": return ElementType.Blast;
            }

            throw new FormatException($"Unknown '{elementType}' element type.");
        }
    }

public class WeaponViewModel : ViewModelBase
    {
        public int Id { get; }
        public string Name { get; }
        public WeaponType Type { get; }
        public int Rarity { get; }
        public int Attack { get; }
        public WeaponAttributesViewModel Attributes { get; }
        public IList<int[]> SharpnessLevels { get; }
        public IList<int> Slots { get; }
        public IList<WeaponElementViewModel> Elements { get; }
        public bool IsCraftable { get; }

        public bool HasAffinity
        {
            get { return Attributes != null && Attributes.Affinity != 0; }
        }

        public bool HasDefense
        {
            get { return Attributes != null && Attributes.Defense != 0; }
        }

        public bool HasDeviation
        {
            get { return Attributes != null && Attributes.DamageType == WeaponDamageType.Projectile && Attributes.Deviation != WeaponDeviation.Unset; }
        }

        public bool HasSharpness
        {
            get { return SharpnessLevels != null && SharpnessLevels.Count > 0; }
        }

        public WeaponViewModel Previous { get; private set; }
        public IList<WeaponViewModel> Branches { get; private set; }

        public bool HasParent { get; private set; }
        public bool HasChildren { get; private set; }

        private bool isHihglight;
        public bool IsHighlight
        {
            get { return isHihglight; }
            set { SetValue(ref isHihglight, value); }
        }

        public WeaponTypeViewModel Parent { get; private set; }

        public WeaponViewModel(WeaponPrimitive primitive)
        {
            Id = primitive.Id;
            Name = primitive.Name;
            Type = ConvertWeaponType(primitive.Type);
            Rarity = primitive.Rarity;
            Attack = primitive.Attack.Display;
            Attributes = new WeaponAttributesViewModel(primitive.Attributes);
            SharpnessLevels = primitive.SharpnessLevels?
                .Select(p => new int[] { p.Red, p.Orange, p.Yellow, p.Green, p.Blue, p.White })
                .ToList();
            Slots = primitive.Slots.Select(x => x.Rank).OrderByDescending(x => x).ToList();
            Elements = primitive.Elements.Select(x => new WeaponElementViewModel(x)).ToList();
            IsCraftable = primitive.Crafting.IsCraftable;
        }

        public void SetParent(WeaponTypeViewModel parent)
        {
            Parent = parent;

            if (Branches != null && Branches.Count > 0)
            {
                foreach (WeaponViewModel child in Branches)
                    child.SetParent(parent);
            }
        }

        private WeaponType ConvertWeaponType(string weaponType)
        {
            switch (weaponType)
            {
                case "great-sword": return WeaponType.GreatSword;
                case "long-sword": return WeaponType.LongSword;
                case "sword-and-shield": return WeaponType.SwordAndShield;
                case "dual-blades": return WeaponType.DualBlades;
                case "hammer": return WeaponType.Hammer;
                case "hunting-horn": return WeaponType.HuntingHorn;
                case "lance": return WeaponType.Lance;
                case "gunlance": return WeaponType.Gunlance;
                case "switch-axe": return WeaponType.SwitchAxe;
                case "charge-blade": return WeaponType.ChargeBlade;
                case "insect-glaive": return WeaponType.InsectGlaive;
                case "light-bowgun": return WeaponType.LightBowgun;
                case "heavy-bowgun": return WeaponType.HeavyBowgun;
                case "bow": return WeaponType.Bow;
            }

            throw new FormatException($"Unknown weapon type '{weaponType}'");
        }

        internal void Update(WeaponViewModel previous, IList<WeaponViewModel> children)
        {
            Previous = previous;

            if (Previous != null)
            {
                HasParent = true;
                if (Previous.Branches == null)
                {
                    // TODO: file data issue to mhw-db.com
                    Previous.Branches = new WeaponViewModel[] { this };
                }
            }

            if (children != null && children.Count > 0)
            {
                Branches = children;
                HasChildren = true;
            }
        }

        public override string ToString()
        {
            return $"{Name} [{Type}] ({Branches?.Count ?? 0})";
        }
    }
}
