using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.ArmoryDataSource.DataStructures;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Core.WPF;
using MHWMasterDataUtils.Core;

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

        public WeaponAttributesViewModel(WeaponBase weapon)
        {
            switch (weapon.Type)
            {
                case MHWMasterDataUtils.Core.WeaponType.GreatSword:
                case MHWMasterDataUtils.Core.WeaponType.LongSword:
                case MHWMasterDataUtils.Core.WeaponType.DualBlades:
                case MHWMasterDataUtils.Core.WeaponType.SwordAndShield:
                case MHWMasterDataUtils.Core.WeaponType.ChargeBlade:
                case MHWMasterDataUtils.Core.WeaponType.SwitchAxe:
                case MHWMasterDataUtils.Core.WeaponType.Lance:
                case MHWMasterDataUtils.Core.WeaponType.Gunlance:
                case MHWMasterDataUtils.Core.WeaponType.InsectGlaive:
                    DamageType = WeaponDamageType.Severe;
                    break;
                case MHWMasterDataUtils.Core.WeaponType.Hammer:
                case MHWMasterDataUtils.Core.WeaponType.HuntingHorn:
                    DamageType = WeaponDamageType.Blunt;
                    break;
                case MHWMasterDataUtils.Core.WeaponType.Bow:
                case MHWMasterDataUtils.Core.WeaponType.LightBowgun:
                case MHWMasterDataUtils.Core.WeaponType.HeavyBowgun:
                    DamageType = WeaponDamageType.Projectile;
                    break;
            }

            switch (weapon.Elderseal)
            {
                case MHWMasterDataUtils.Core.Elderseal.Low:
                    Elderseal = WeaponElderseal.Low;
                    break;
                case MHWMasterDataUtils.Core.Elderseal.Average:
                    Elderseal = WeaponElderseal.Average;
                    break;
                case MHWMasterDataUtils.Core.Elderseal.High:
                    Elderseal = WeaponElderseal.High;
                    break;
                default:
                    Elderseal = WeaponElderseal.None;
                    break;
            }

            if (weapon is Bowgun bowgun)
            {
                switch (bowgun.Deviation)
                {
                    case BowgunDeviation.None:
                        Deviation = WeaponDeviation.None;
                        break;
                    case BowgunDeviation.Low:
                        Deviation = WeaponDeviation.Low;
                        break;
                    case BowgunDeviation.Average:
                        Deviation = WeaponDeviation.Average;
                        break;
                    case BowgunDeviation.High:
                        Deviation = WeaponDeviation.High;
                        break;
                    default:
                        Deviation = WeaponDeviation.Unset;
                        break;
                }
            }

            Affinity = weapon.Affinity;
            Defense = weapon.Defense;
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

        public WeaponElementViewModel(int damage, bool isHidden, ElementStatus elementStatus)
        {
            originalValue = damage;
            originalIsHidden = isHidden;

            Type = ConvertElementType(elementStatus);

            Value = originalValue;
            IsHidden = originalIsHidden;
        }

        private ElementType ConvertElementType(ElementStatus elementStatus)
        {
            switch (elementStatus)
            {
                case ElementStatus.Fire: return ElementType.Fire;
                case ElementStatus.Water: return ElementType.Water;
                case ElementStatus.Thunder: return ElementType.Thunder;
                case ElementStatus.Ice: return ElementType.Ice;
                case ElementStatus.Dragon: return ElementType.Dragon;
                case ElementStatus.Poison: return ElementType.Poison;
                case ElementStatus.Sleep: return ElementType.Sleep;
                case ElementStatus.Paralysis: return ElementType.Paralysis;
                case ElementStatus.Blast: return ElementType.Blast;
            }

            throw new FormatException($"Unknown '{elementStatus}' element type.");
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
        public Dictionary<string, string> Name { get; }
        public MHWMasterDataUtils.Core.WeaponType Type { get; }
        public int Rarity { get; }
        public int Attack { get; }
        public WeaponAttributesViewModel Attributes { get; }
        public List<ushort[]> SharpnessLevels { get; }
        public IList<WeaponElementViewModel> Elements { get; }
        public bool IsCraftable { get; }

        private bool isPossessed;
        public bool IsPossessed
        {
            get { return isPossessed; }
            private set { SetValue(ref isPossessed, value); }
        }

        private bool isPossessedMoreThanOnce;
        public bool IsPossessedMoreThanOnce
        {
            get { return isPossessedMoreThanOnce; }
            private set { SetValue(ref isPossessedMoreThanOnce, value); }
        }

        private int possessedCount;
        public int PossessedCount
        {
            get { return possessedCount; }
            set
            {
                if (SetValue(ref possessedCount, value))
                {
                    IsPossessed = possessedCount > 0;
                    IsPossessedMoreThanOnce = possessedCount > 1;
                }
            }
        }

        private readonly IList<int> originalSlots;

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
            get { return SharpnessLevels != null; }
        }

        public WeaponViewModel Previous { get; private set; }
        public List<WeaponViewModel> Branches { get; private set; }

        public bool HasParent { get; private set; }
        public bool HasChildren { get; private set; }

        private bool isHihglight;
        public bool IsHighlight
        {
            get { return isHihglight; }
            set { SetValue(ref isHihglight, value); }
        }

        private bool isFilteredText;
        public bool IsFilteredText
        {
            get { return isFilteredText; }
            private set { SetValue(ref isFilteredText, value); }
        }

        public int SortIndex { get; }

        public WeaponTypeViewModel Parent { get; private set; }

        public WeaponViewModel(WeaponBase weapon)
        {
            Id = (int)weapon.Id;
            Name = weapon.Name;
            Type = weapon.Type;
            Rarity = weapon.Rarity;
            Attack = weapon.Damage;
            Attributes = new WeaponAttributesViewModel(weapon);

            SortIndex = weapon.SortOrder;

            if (weapon is MeleeWeapon meleeWeapon)
                SharpnessLevels = meleeWeapon.Sharpness.Select(x => x.ToArray()).ToList();

            Elements = CreateElements(weapon);

            IsCraftable = weapon.Craft?.IsCraftable ?? false;

            originalSlots = weapon.Slots.OrderByDescending(x => x).ToList();
            Slots = originalSlots;
        }

        private static WeaponElementViewModel[] CreateElements(WeaponBase weapon)
        {
            var result = new List<WeaponElementViewModel>();

            if (weapon.ElementStatus != ElementStatus.None || weapon.HiddenElementStatus != ElementStatus.None)
            {
                int value = weapon.ElementStatusDamage > 0 ? weapon.ElementStatusDamage : weapon.HiddenElementStatusDamage;
                bool isHidden = weapon.HiddenElementStatusDamage > 0;

                ElementStatus elementStatus = weapon.ElementStatusDamage > 0 ? weapon.ElementStatus : weapon.HiddenElementStatus;

                result.Add(new WeaponElementViewModel(value, isHidden, elementStatus));

                if (weapon is DualBlades dualBlades && dualBlades.SecondaryElementStatus.HasValue)
                {
                    value = dualBlades.SecondaryElementStatusDamage.Value;
                    elementStatus = dualBlades.SecondaryElementStatus.Value;

                    result.Add(new WeaponElementViewModel(value, false, elementStatus));
                }
            }

            return result.ToArray();
        }

        internal void SetParentType(WeaponTypeViewModel parent)
        {
            Parent = parent;

            if (Branches != null && Branches.Count > 0)
            {
                foreach (WeaponViewModel child in Branches)
                    child.SetParentType(parent);
            }
        }

        internal void SetParent(WeaponViewModel parent)
        {
            Previous = parent;
            parent.AddChild(this);
        }

        internal void AddChild(WeaponViewModel child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            if (Branches == null)
                Branches = new List<WeaponViewModel>();

            Branches.Add(child);
        }

        public override string ToString()
        {
            return $"{Name} [{Type}] ({Branches?.Count ?? 0})";
        }

        public void ClearFiltered()
        {
            IsFilteredText = false;

            if (Branches != null)
            {
                foreach (WeaponViewModel x in Branches)
                    x.ClearFiltered();
            }
        }

        public void UpdateFiltered(SearchStatement st)
        {
            IsFilteredText = st.IsMatching(Localization.Get(Name));

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
