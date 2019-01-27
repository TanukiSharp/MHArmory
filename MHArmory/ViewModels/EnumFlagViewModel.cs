using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public class EnumFlagViewModel<T> : ViewModelBase
    {
        public string Name { get; }

        private bool isSet;
        public bool IsSet
        {
            get { return isSet; }
            set
            {
                if (SetValue(ref isSet, value))
                    notifyChanged(value, enumValue);
            }
        }

        private readonly T enumValue;
        private readonly Action<bool, T> notifyChanged;

        public EnumFlagViewModel(string name, bool initialValue, T enumValue, Action<bool, T> onChanged)
        {
            Name = name;
            isSet = initialValue;
            this.enumValue = enumValue;
            notifyChanged = onChanged;
        }
    }
}
