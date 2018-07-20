using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.ViewModels
{
    public class InParametersViewModel : ViewModelBase
    {
        public int[] Slots { get; } = new int[3];
    }
}
