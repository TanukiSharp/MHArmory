using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
        private readonly RootViewModel root;

        public ValueViewModel<int>[] Slots { get; }

        private bool useOverride;
        public bool UseOverride
        {
            get { return useOverride; }
            set
            {
                if (SetValue(ref useOverride, value))
                    UseOverrideChanged();
            }
        }

        public ICommand OpenDecorationsOverrideCommand { get { return root.OpenDecorationsOverrideCommand; } }

        public InParametersViewModel(RootViewModel root)
        {
            this.root = root;

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

                if (config.WeaponSlots != null)
                {
                    for (int i = 0; i < Slots.Length && i < config.WeaponSlots.Length; i++)
                        Slots[i].Value = config.WeaponSlots[i];
                }

                UseOverride = config.DecorationOverride.UseOverride;
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

            InParametersConfiguration config = GlobalData.Instance.Configuration.InParameters;

            config.WeaponSlots = Slots.Select(x => x.Value).ToArray();
            GlobalData.Instance.Configuration.Save();

            root.CreateSolverData();
        }

        private void UseOverrideChanged()
        {
            if (isLoadingConfiguration)
                return;

            InParametersConfiguration config = GlobalData.Instance.Configuration.InParameters;

            config.DecorationOverride.UseOverride = UseOverride;
            GlobalData.Instance.Configuration.Save();

            root.CreateSolverData();
        }
    }
}
