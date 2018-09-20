using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.ArmoryDataSource.DataStructures;

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
        public string Type { get; }
        public int Rarity { get; }
        public int Attack { get; }
        public WeaponAttributesViewModel Attributes { get; }
        public IList<int[]> SharpnessLevels { get; set; }

        public bool IsCraftable { get; }
        public WeaponViewModel Previous { get; private set; }
        public WeaponViewModel[] Branches { get; private set; }

        public bool HasParent { get; private set; }
        public bool HasChildren { get; private set; }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetValue(ref isExpanded, value); }
        }

        public WeaponViewModel(WeaponPrimitive primitive)
        {
            Id = primitive.Id;
            Name = primitive.Name;
            Type = primitive.Type;
            Rarity = primitive.Rarity;
            Attack = primitive.Attack.Display;
            Attributes = null;
            SharpnessLevels = primitive.SharpnessLevels?
                .Select(p => new int[] { p.Red, p.Orange, p.Yellow, p.Green, p.Blue, p.White })
                .ToList();
            IsCraftable = primitive.Crafting.IsCraftable;
        }

        internal void Update(WeaponViewModel previous, WeaponViewModel[] children)
        {
            Previous = previous;

            if (Previous != null)
            {
                HasParent = true;
                if (Previous.Branches == null)
                {
                    // TODO: file data issue to mhw-db.com
                }
            }

            if (children != null && children.Length > 0)
            {
                Branches = children;
                HasChildren = true;
            }
        }

        public override string ToString()
        {
            return $"{Name} [{Type}] ({Branches?.Length ?? 0})";
        }
    }
}
