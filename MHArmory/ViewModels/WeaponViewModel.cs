using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.ArmoryDataSource.DataStructures;
using MHArmory.Core.DataStructures;
using MHArmory.Core.WPF;

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

    public class WeaponElementViewModel : ViewModelBase
    {
        private readonly int originalValue;
        private readonly bool originalIsHidden;

        public ElementType Type { get; }

        private int value;
        public int Value
        {
            get { return value; }
            private set { SetValue(ref this.value, value); }
        }

        private bool isHidden;
        public bool IsHidden
        {
            get { return isHidden; }
            private set { SetValue(ref isHidden, value); }
        }

        public WeaponElementViewModel(WeaponElementPrimitive primitive)
        {
            originalValue = primitive.Value;
            originalIsHidden = primitive.IsHidden;

            Type = ConvertElementType(primitive.Type);

            Value = originalValue;
            IsHidden = originalIsHidden;
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

        public void FreeElementSkillChanged(int level)
        {
            if (originalIsHidden == false)
                return;

            if (level <= 0)
            {
                IsHidden = originalIsHidden;
                Value = originalValue;
            }
            else
            {
                IsHidden = false;
                Value = originalValue * level / 3;
            }
        }
    }

    public class WeaponViewModel : ViewModelBase
    {
        public int Id { get; }
        public string Name { get; } // TODO: localization here
        public WeaponType Type { get; }
        public int Rarity { get; }
        public int Attack { get; }
        public WeaponAttributesViewModel Attributes { get; }
        public IList<int[]> SharpnessLevels { get; }
        public IList<WeaponElementViewModel> Elements { get; }
        public bool IsCraftable { get; }

        private IList<int> originalSlots;

        private IList<int> slots;
        public IList<int> Slots
        {
            get { return slots; }
            private set { SetValue(ref slots, value); }
        }

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

        private bool isFiltered;
        public bool IsFiltered
        {
            get { return isFiltered; }
            private set { SetValue(ref isFiltered, value); }
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
            Elements = primitive.Elements.Select(x => new WeaponElementViewModel(x)).ToList();
            IsCraftable = primitive.Crafting.IsCraftable;

            originalSlots = primitive.Slots.Select(x => x.Rank).OrderByDescending(x => x).ToList();
            Slots = originalSlots;
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

        public void ClearFiltered()
        {
            IsFiltered = false;

            if (Branches != null)
            {
                foreach (WeaponViewModel x in Branches)
                    x.ClearFiltered();
            }
        }

        public void UpdateFiltered(SearchStatement st)
        {
            IsFiltered = st.IsMatching(Name);

            if (Branches != null)
            {
                foreach (WeaponViewModel x in Branches)
                    x.UpdateFiltered(st);
            }
        }

        public void FreeElementSkillChanged(int level)
        {
            if (Elements != null)
            {
                foreach (WeaponElementViewModel x in Elements)
                    x.FreeElementSkillChanged(level);
            }

            if (Branches != null)
            {
                foreach (WeaponViewModel x in Branches)
                    x.FreeElementSkillChanged(level);
            }
        }

        public void SlotAugmentationCountChanged(int count)
        {
            if (Branches != null && Branches.Count > 0)
            {
                foreach (WeaponViewModel child in Branches)
                    child.SlotAugmentationCountChanged(count);
                return;
            }

            if (count <= 0)
                Slots = originalSlots;
            else if (originalSlots == null || originalSlots.Count == 0)
                Slots = new int[] { count };
            else
            {
                int[] newSlots = new int[originalSlots.Count + 1];
                for (int i = 0; i < originalSlots.Count; i++)
                    newSlots[i] = Slots[i];
                newSlots[newSlots.Length - 1] = count;
                Slots = newSlots;
            }
        }
    }
}
