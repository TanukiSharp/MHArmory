using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.ViewModels
{
    public class ValueViewModel<T> : ViewModelBase
    {
        private readonly Action<T> notifyChanged;

        private T value;
        public T Value
        {
            get { return value; }
            set
            {
                if (SetValue(ref this.value, value) && notifyChanged != null)
                    notifyChanged(value);
            }
        }

        public ValueViewModel()
            : this(default(T), null)
        {
        }

        public ValueViewModel(T initialValue)
            : this(initialValue, null)
        {
        }

        public ValueViewModel(Action<T> notifyChanged)
            : this(default(T), notifyChanged)
        {
        }

        public ValueViewModel(T initialValue, Action<T> notifyChanged)
        {
            value = initialValue;
            this.notifyChanged = notifyChanged;
        }
    }

    public class InParametersViewModel : ViewModelBase
    {
        public ValueViewModel<int>[] Slots { get; }

        public InParametersViewModel()
        {
            Slots = new ValueViewModel<int>[3];
            for (int i = 0; i < Slots.Length; i++)
                Slots[i] = new ValueViewModel<int>(WeaponSlotsChanged);
        }

        private bool isLoadingConfiguration;

        internal void NotifyConfigurationLoaded()
        {
            isLoadingConfiguration = true;

            try
            {
                InParametersConfiguration config = GlobalData.Instance.Configuration.InParameters;

                if (config.WeaponSlots == null)
                    return;

                for (int i = 0; i < Slots.Length && i < config.WeaponSlots.Length; i++)
                    Slots[i].Value = config.WeaponSlots[i];
            }
            finally
            {
                isLoadingConfiguration = false;
            }
        }

        private void WeaponSlotsChanged(int value)
        {
            if (isLoadingConfiguration)
                return;

            GlobalData.Instance.Configuration.InParameters.WeaponSlots = Slots.Select(x => x.Value).ToArray();
            GlobalData.Instance.Configuration.Save();
        }
    }
}
