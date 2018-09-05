using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Configurations;

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

        private int rarityIndex;
        public int RarityIndex
        {
            get { return rarityIndex; }
            set
            {
                if (SetValue(ref rarityIndex, value))
                    Rarity = RarityIndex + 1;
            }
        }

        private int rarity;
        public int Rarity
        {
            get { return rarity; }
            set
            {
                if (SetValue(ref rarity, value))
                {
                    RarityIndex = Rarity - 1;
                    RarityChanged();
                }
            }
        }

        private int genderIndex;
        public int GenderIndex
        {
            get { return genderIndex; }
            set
            {
                if (SetValue(ref genderIndex, value))
                    Gender = (Core.DataStructures.Gender)GenderIndex + 1;
            }
        }

        private Core.DataStructures.Gender gender;
        public Core.DataStructures.Gender Gender
        {
            get { return gender; }
            set
            {
                if (SetValue(ref gender, value))
                {
                    GenderIndex = (int)Gender - 1;
                    GenderChanged();
                }
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
                InParametersConfigurationV2 config = GlobalData.Instance.Configuration.InParameters;

                if (config.WeaponSlots != null)
                {
                    for (int i = 0; i < Slots.Length && i < config.WeaponSlots.Length; i++)
                        Slots[i].Value = config.WeaponSlots[i];
                }

                UseOverride = config.DecorationOverride.UseOverride;

                if (config.Rarity <= 0)
                    Rarity = 9; // initial rarity, maximum one
                else
                    Rarity = config.Rarity;

                if (config.Gender < 0 || config.Gender > 2)
                    Gender = Core.DataStructures.Gender.Both;
                else
                    GenderIndex = config.Gender;
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

            InParametersConfigurationV2 config = GlobalData.Instance.Configuration.InParameters;

            config.WeaponSlots = Slots.Select(x => x.Value).ToArray();
            ConfigurationManager.Save(GlobalData.Instance.Configuration);

            root.CreateSolverData();
        }

        private void UseOverrideChanged()
        {
            if (isLoadingConfiguration)
                return;

            InParametersConfigurationV2 config = GlobalData.Instance.Configuration.InParameters;

            config.DecorationOverride.UseOverride = UseOverride;
            ConfigurationManager.Save(GlobalData.Instance.Configuration);

            root.CreateSolverData();
        }

        private void RarityChanged()
        {
            if (isLoadingConfiguration)
                return;

            InParametersConfigurationV2 config = GlobalData.Instance.Configuration.InParameters;

            config.Rarity = Rarity;
            ConfigurationManager.Save(GlobalData.Instance.Configuration);

            root.CreateSolverData();
        }

        private void GenderChanged()
        {
            if (isLoadingConfiguration)
                return;

            InParametersConfigurationV2 config = GlobalData.Instance.Configuration.InParameters;

            config.Gender = GenderIndex;
            ConfigurationManager.Save(GlobalData.Instance.Configuration);

            root.CreateSolverData();
        }
    }
}
