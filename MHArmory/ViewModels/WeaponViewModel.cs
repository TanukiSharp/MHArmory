using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.ArmoryDataSource.DataStructures;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class WeaponAttributesViewModel
    {
        public string DamageType { get; }
        public string Elderseal { get; }
        public int Affinity { get; }
        public int Defense { get; }
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
        public bool IsCraftable { get; }

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

        public WeaponViewModel(WeaponPrimitive primitive)
        {
            Id = primitive.Id;
            Name = primitive.Name;
            Type = ConvertWeaponType(primitive.Type);
            Rarity = primitive.Rarity;
            Attack = primitive.Attack.Display;
            Attributes = null;
            SharpnessLevels = primitive.SharpnessLevels?
                .Select(p => new int[] { p.Red, p.Orange, p.Yellow, p.Green, p.Blue, p.White })
                .ToList();
            Slots = primitive.Slots.Select(x => x.Rank).OrderByDescending(x => x).ToList();
            IsCraftable = primitive.Crafting.IsCraftable;
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
