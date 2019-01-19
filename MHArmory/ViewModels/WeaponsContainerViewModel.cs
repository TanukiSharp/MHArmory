using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.ArmoryDataSource.DataStructures;
using MHArmory.Core.WPF;
using Newtonsoft.Json;

namespace MHArmory.ViewModels
{
    public class WeaponsContainerViewModel : ViewModelBase
    {
        private readonly RootViewModel rootViewModel;

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
                weaponType.IsActive = weaponType == weaponTypeToActivate;
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

        public void UpdateHighlights()
        {
            IList<int> inputSlots = rootViewModel.InParameters.Slots
                .Select(x => x.Value)
                .Where(x => x > 0)
                .OrderByDescending(x => x)
                .ToList();

            if (allWeapons != null)
            {
                foreach (WeaponViewModel weapon in allWeapons.Values)
                    weapon.IsHighlight = IsWeaponMatchingSlots(weapon, inputSlots);
            }
        }

        private bool IsWeaponMatchingSlots(WeaponViewModel weapon, IList<int> inputSlots)
        {
            if (weapon.Slots.Count < inputSlots.Count)
                return false;

            IList<int> sortedSlots = weapon.Slots.OrderByDescending(x => x).ToList();

            int count = inputSlots.Count;
            for (int i = 0; i < count; i++)
            {
                if (sortedSlots[i] < inputSlots[i])
                    return false;
            }

            return true;
        }

        public void ActivateDefaultIfNeeded()
        {
            if (WeaponTypes == null || WeaponTypes.Count == 0)
                return;

            if (WeaponTypes.All(x => x.IsActive == false))
                WeaponTypes[0].IsActive = true;
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

        private Task<IList<WeaponPrimitive>> LoadPrimitives(IList<string> filenames)
        {
            IList<WeaponPrimitive> LoadPrimitivesInternal()
            {
                var weaponPrimitives = new List<WeaponPrimitive>();

                foreach (string filename in filenames)
                {
                    string weaponsContent = File.ReadAllText(filename);
                    weaponPrimitives.AddRange(JsonConvert.DeserializeObject<IEnumerable<WeaponPrimitive>>(weaponsContent));
                }

                return weaponPrimitives;
            }

            return Task.Factory.StartNew(LoadPrimitivesInternal, TaskCreationOptions.LongRunning);
        }

        private static readonly string[] weaponFilenames = new string[]
        {
            "great-sword",
            "long-sword",
            "sword-and-shield",
            "dual-blades",
            "hammer",
            "hunting-horn",
            "lance",
            "gunlance",
            "switch-axe",
            "charge-blade",
            "insect-glaive",
            "light-bowgun",
            "heavy-bowgun",
            "bow"
        };

        private async Task<bool> LoadWeaponsInternal()
        {
            var fullFilenames = new List<string>();

            foreach (string filename in weaponFilenames)
            {
                string fullFilename = Path.Combine(AppContext.BaseDirectory, "data", $"{filename}.json");

                if (File.Exists(fullFilename) == false)
                {
                    IsFeatureEnabled = false;
                    FeatureDisabledReason = $"Weapons data unavailable ('{filename}' missing)";
                    return false;
                }

                fullFilenames.Add(fullFilename);
            }

            IList<WeaponPrimitive> allWeaponPrimitives = await LoadPrimitives(fullFilenames);

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
