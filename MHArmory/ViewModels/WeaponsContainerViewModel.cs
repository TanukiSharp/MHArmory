using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.ArmoryDataSource.DataStructures;
using Newtonsoft.Json;

namespace MHArmory.ViewModels
{
    public class WeaponsContainerViewModel : ViewModelBase
    {
        private readonly RootViewModel rootViewModel;

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

        private bool isFeatureEnabled;
        public bool IsFeatureEnabled
        {
            get { return isFeatureEnabled; }
            private set { SetValue(ref isFeatureEnabled, value); }
        }

        private string featureDisabledReason;
        public string FeatureDisabledReason
        {
            get { return featureDisabledReason; }
            private set { SetValue(ref featureDisabledReason, value); }
        }

        private bool hasWeapons;
        public bool HasWeapons
        {
            get { return hasWeapons; }
            private set { SetValue(ref hasWeapons, value); }
        }

        private ICollection<WeaponViewModel> weapons;
        public ICollection<WeaponViewModel> Weapons
        {
            get { return weapons; }
            set
            {
                if (SetValue(ref weapons, value))
                    HasWeapons = Weapons != null && Weapons.Count > 0;
            }
        }

        public WeaponsContainerViewModel(RootViewModel rootViewModel)
        {
            this.rootViewModel = rootViewModel;
        }

        public async Task LoadWeaponsAsync()
        {
            try
            {
                if (await LoadWeaponsInternal())
                {
                    IsFeatureEnabled = true;
                    FeatureDisabledReason = null;
                }
            }
            catch (Exception ex)
            {
                IsFeatureEnabled = false;
                FeatureDisabledReason = $"Error: {ex.Message}";
            }
        }

        private Task<IList<WeaponPrimitive>> LoadPrimitives(string filename)
        {
            IList<WeaponPrimitive> LoadPrimitivesInternal()
            {
                string weaponsContent = File.ReadAllText(filename);
                return JsonConvert.DeserializeObject<IList<WeaponPrimitive>>(weaponsContent);
            }

            return Task.Factory.StartNew(LoadPrimitivesInternal, TaskCreationOptions.LongRunning);
        }

        private async Task<bool> LoadWeaponsInternal()
        {
            string filename = Path.Combine(AppContext.BaseDirectory, "data", "weapons.json");

            if (File.Exists(filename) == false)
            {
                IsFeatureEnabled = false;
                FeatureDisabledReason = $"Weapons data unavailable";
                return false;
            }

            IList<WeaponPrimitive> weapons = await LoadPrimitives(filename);

            IDictionary<int, WeaponViewModel> allWeaponViewModels = weapons
                .Select(x => new WeaponViewModel(x))
                .ToDictionary(x => x.Id, x => x);

            IList<WeaponViewModel> roots = weapons
                .Where(x => x.Crafting.Previous == null)
                .Select(x => allWeaponViewModels[x.Id])
                .ToList();

            foreach (WeaponPrimitive primitive in weapons)
            {
                WeaponViewModel weapon = allWeaponViewModels[primitive.Id];

                WeaponViewModel previous = null;
                if (primitive.Crafting.Previous.HasValue)
                    previous = allWeaponViewModels[primitive.Crafting.Previous.Value];

                weapon.Update(previous, primitive.Crafting.Branches.Select(id => allWeaponViewModels[id]).ToArray());
            }

            Weapons = roots;

            if (HasWeapons == false)
            {
                IsFeatureEnabled = false;
                FeatureDisabledReason = "No weapons available";
            }

            return true;
        }
    }
}
