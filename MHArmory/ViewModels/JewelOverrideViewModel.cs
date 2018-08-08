using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class JewelOverrideViewModel : ViewModelBase
    {
        private readonly IJewel jewel;

        public string Name { get; }
        public int SlotSize { get; }
        public IAbility[] Abilities { get; }

        private int count;
        public int Count
        {
            get { return count; }
            set { SetValue(ref count, value); }
        }

        private bool isOverriding;
        public bool IsOverriding
        {
            get { return isOverriding; }
            set { SetValue(ref isOverriding, value); }
        }

        public JewelOverrideViewModel(IJewel jewel, int count)
        {
            Name = jewel.Name;
            SlotSize = jewel.SlotSize;
            Abilities = jewel.Abilities;
        }
    }
}
