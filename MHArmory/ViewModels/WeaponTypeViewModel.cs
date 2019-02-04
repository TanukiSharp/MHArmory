using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core.DataStructures;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public class WeaponTypeViewModel : ViewModelBase
    {
        public WeaponsContainerViewModel Parent { get; }

        private readonly IList<WeaponViewModel> awaitingRootWeapons;

        public WeaponType Type { get; }

        private bool isDataLoading;
        public bool IsDataLoading
        {
            get { return isDataLoading; }
            set { SetValue(ref isDataLoading, value); }
        }

        private bool isDataLoaded;
        public bool IsDataLoaded
        {
            get { return isDataLoaded; }
            set { SetValue(ref isDataLoaded, value); }
        }

        private IList<WeaponViewModel> rootWeapons;
        public IList<WeaponViewModel> RootWeapons
        {
            get { return rootWeapons; }
            private set { SetValue(ref rootWeapons, value); }
        }

        public ICommand ActivateCommand { get; }

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (SetValue(ref isActive, value) && IsActive && RootWeapons == null)
                    SetRootWeapons();
            }
        }

        public bool IsSharpnessWeapon { get; private set; }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (SetValue(ref searchText, value))
                    OnSearchTextChanged();
            }
        }

        private int freeElementLevel;
        public int FreeElementLevel
        {
            get { return freeElementLevel; }
            set
            {
                if (SetValue(ref freeElementLevel, value))
                {
                    foreach (WeaponViewModel x in RootWeapons)
                        x.FreeElementSkillChanged(FreeElementLevel);
                }
            }
        }

        private int sharpnessRank;
        public int SharpnessRank
        {
            get { return sharpnessRank; }
            set { SetValue(ref sharpnessRank, value); }
        }

        private int slotAugmentationCount;
        public int SlotAugmentationCount
        {
            get { return slotAugmentationCount; }
            set
            {
                if (SetValue(ref slotAugmentationCount, value))
                {
                    foreach (WeaponViewModel x in RootWeapons)
                        x.SlotAugmentationCountChanged(slotAugmentationCount);
                    Parent.UpdateHighlights();
                }
            }
        }

        public WeaponTypeViewModel(WeaponType type, IList<WeaponViewModel> rootWeapons, WeaponsContainerViewModel parent)
        {
            Parent = parent;

            Type = type;
            awaitingRootWeapons = rootWeapons;

            SetIsSharpness();

            ActivateCommand = new AnonymousCommand(OnActivate);

            foreach (WeaponViewModel weapon in rootWeapons)
                weapon.SetParent(this);
        }

        private async void SetRootWeapons()
        {
            if (IsDataLoading)
                return;

            IsDataLoaded = false;
            IsDataLoading = true;

            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.SystemIdle);

            RootWeapons = awaitingRootWeapons;

            IsDataLoaded = true;
            IsDataLoading = false;
        }

        private void SetIsSharpness()
        {
            switch (Type)
            {
                case WeaponType.GreatSword:
                case WeaponType.LongSword:
                case WeaponType.SwordAndShield:
                case WeaponType.DualBlades:
                case WeaponType.Hammer:
                case WeaponType.HuntingHorn:
                case WeaponType.Lance:
                case WeaponType.Gunlance:
                case WeaponType.SwitchAxe:
                case WeaponType.ChargeBlade:
                case WeaponType.InsectGlaive:
                    IsSharpnessWeapon = true;
                    return;
            }

            IsSharpnessWeapon = false;
        }

        private void OnSearchTextChanged()
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (WeaponViewModel x in RootWeapons)
                    x.ClearFiltered();
            }
            else
            {
                var searchStatement = SearchStatement.Create(searchText, GlobalData.Instance.Aliases);

                foreach (WeaponViewModel x in RootWeapons)
                    x.UpdateFiltered(searchStatement);
            }
        }

        private void OnActivate(object parameters)
        {
            Parent.Activate(this);
        }
    }
}
