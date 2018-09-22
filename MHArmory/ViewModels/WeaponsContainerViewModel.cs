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

        internal void Activate(WeaponTypeViewModel weaponTypeToActivate)
        {
            foreach (WeaponTypeViewModel weaponType in WeaponTypes)
                weaponType.IsVisible = weaponType == weaponTypeToActivate;
        }

        private IDictionary<int, WeaponViewModel> allWeapons;

        private IList<WeaponTypeViewModel> weaponTypes;
        public IList<WeaponTypeViewModel> WeaponTypes
        {
            get { return weaponTypes; }
            set { SetValue(ref weaponTypes, value); }
        }

        public WeaponsContainerViewModel(RootViewModel rootViewModel)
        {
            this.rootViewModel = rootViewModel;
        }

        public void UpdateHighlights(IList<int> inputSlots)
        {
            foreach (WeaponViewModel weapon in allWeapons.Values)
                weapon.IsHighlight = IsWeaponMatchingSlots(weapon, inputSlots);
        }

        private bool IsWeaponMatchingSlots(WeaponViewModel weapon, IList<int> inputSlots)
        {
            if (weapon.Slots.Count < inputSlots.Count)
                return false;

            int count = inputSlots.Count;
            for (int i = 0; i < count; i++)
            {
                if (weapon.Slots[i] < inputSlots[i])
                    return false;
            }

            return true;
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

            IList<WeaponPrimitive> allWeaponPrimitives = await LoadPrimitives(filename);

            allWeapons = allWeaponPrimitives
                .Select(x => new WeaponViewModel(x))
                .ToDictionary(x => x.Id, x => x);

            bool hasRoots = false;

            foreach (WeaponPrimitive primitive in allWeaponPrimitives)
            {
                WeaponViewModel weapon = allWeapons[primitive.Id];

                WeaponViewModel previous = null;
                if (primitive.Crafting.Previous.HasValue)
                    previous = allWeapons[primitive.Crafting.Previous.Value];

                if (previous == null)
                    hasRoots = true;

                weapon.Update(previous, primitive.Crafting.Branches.Select(id => allWeapons[id]).ToList());
            }

            if (hasRoots == false)
            {
                IsFeatureEnabled = false;
                FeatureDisabledReason = "No weapons available";
            }

            WeaponTypes = allWeaponPrimitives
                .Where(x => x.Crafting.Previous == null)
                .Select(x => allWeapons[x.Id])
                .GroupBy(x => x.Type)
                .Select(x => new WeaponTypeViewModel(x.Key, x.ToList(), this))
                .ToList();

            return true;
        }
    }
}
