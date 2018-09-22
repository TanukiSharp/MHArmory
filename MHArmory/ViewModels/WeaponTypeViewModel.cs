using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class WeaponTypeViewModel : ViewModelBase
    {
        private readonly WeaponsContainerViewModel parent;

        public WeaponType Type { get; }
        public IList<WeaponViewModel> RootWeapons { get; }

        public ICommand ActivateCommand { get; }

        private bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        public WeaponTypeViewModel(WeaponType type, IList<WeaponViewModel> rootWeapons, WeaponsContainerViewModel parent)
        {
            this.parent = parent;

            Type = type;
            RootWeapons = rootWeapons;

            ActivateCommand = new AnonymousCommand(OnActivate);
        }

        private void OnActivate(object parameters)
        {
            parent.Activate(this);
        }
    }
}
