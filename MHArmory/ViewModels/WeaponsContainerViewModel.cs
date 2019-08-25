using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.ArmoryDataSource.DataStructures;
using MHArmory.Core.WPF;
using Newtonsoft.Json;
using MHWMasterDataUtils.Core;

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

        private string featureDisabledReason = string.Empty;
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

        private IDictionary<(WeaponType, int), WeaponViewModel> allWeapons;

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

        private static WeaponType ConvertWeaponType(MHWSaveUtils.WeaponType weaponType)
        {
            switch (weaponType)
            {
                case MHWSaveUtils.WeaponType.GreatSword: return WeaponType.GreatSword;
                case MHWSaveUtils.WeaponType.SwordAndShield: return WeaponType.SwordAndShield;
                case MHWSaveUtils.WeaponType.DualBlades: return WeaponType.DualBlades;
                case MHWSaveUtils.WeaponType.LongSword: return WeaponType.LongSword;
                case MHWSaveUtils.WeaponType.Hammer: return WeaponType.Hammer;
                case MHWSaveUtils.WeaponType.HuntingHorn: return WeaponType.HuntingHorn;
                case MHWSaveUtils.WeaponType.Lance: return WeaponType.Lance;
                case MHWSaveUtils.WeaponType.Gunlance: return WeaponType.Gunlance;
                case MHWSaveUtils.WeaponType.SwitchAxe: return WeaponType.SwitchAxe;
                case MHWSaveUtils.WeaponType.ChargeBlade: return WeaponType.ChargeBlade;
                case MHWSaveUtils.WeaponType.InsectGlaive: return WeaponType.InsectGlaive;
                case MHWSaveUtils.WeaponType.Bow: return WeaponType.Bow;
                case MHWSaveUtils.WeaponType.HeavyBowgun: return WeaponType.HeavyBowgun;
                case MHWSaveUtils.WeaponType.LightBowgun: return WeaponType.LightBowgun;
            }

            throw new ArgumentException($"Unknown '{nameof(weaponType)}' argument value '{weaponType}'.");
        }

        public void UpdateSaveData(MHWSaveUtils.EquipmentsSaveSlotInfo saveData)
        {
            Dictionary<(WeaponType, int), int> saveDataWeapons = saveData.Equipments
                .Where(x => x.Type == MHWSaveUtils.EquipmentType.Weapon)
                .GroupBy(x => (ConvertWeaponType(x.WeaponType), (int)x.ClassId))
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (WeaponViewModel weapon in allWeapons.Values)
            {
                if (saveDataWeapons.TryGetValue((weapon.Type, weapon.SortIndex), out int count))
                {
                    if (count > 0)
                        weapon.IsPossessed = true;
                }
            }
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

        private Task<List<WeaponBase>> LoadWeaponPrimitives()
        {
            List<WeaponBase> LoadPrimitivesInternal()
            {
                var result = new List<WeaponBase>();

                LoadWeapons<MeleeWeapon>(WeaponType.GreatSword, "great-swords", result);
                LoadWeapons<MeleeWeapon>(WeaponType.LongSword, "long-swords", result);
                LoadWeapons<DualBlades>(WeaponType.DualBlades, "dual-blades", result);
                LoadWeapons<MeleeWeapon>(WeaponType.SwordAndShield, "sword-and-shields", result);
                LoadWeapons<MeleeWeapon>(WeaponType.Hammer, "hammers", result);
                LoadWeapons<HuntingHorn>(WeaponType.HuntingHorn, "hunting-horns", result);
                LoadWeapons<MeleeWeapon>(WeaponType.Lance, "lances", result);
                LoadWeapons<Gunlance>(WeaponType.Gunlance, "gunlances", result);
                LoadWeapons<SwitchAxe>(WeaponType.SwitchAxe, "switch-axes", result);
                LoadWeapons<ChargeBlade>(WeaponType.ChargeBlade, "charge-blades", result);
                LoadWeapons<InsectGlaive>(WeaponType.InsectGlaive, "insect-glaives", result);
                LoadWeapons<Bow>(WeaponType.Bow, "bows", result);
                LoadWeapons<Bowgun>(WeaponType.HeavyBowgun, "heavy-bowguns", result);
                LoadWeapons<Bowgun>(WeaponType.LightBowgun, "light-bowguns", result);

                return result;
            }

            return Task.Factory.StartNew(LoadPrimitivesInternal, TaskCreationOptions.LongRunning);
        }

        private static string ConvertToLegacyLocalizationKey(string newLocalizationKey)
        {
            switch (newLocalizationKey)
            {
                case "eng": return "EN";
                case "fre": return "FR";
                case "jpn": return "JP";
                case "ita": return "IT";
                case "ger": return "DE";
                case "kor": return "KR";
                case "cht": return "CN";
            }

            return null;
        }

        private static Dictionary<string, string> ConvertToLegacyLocalization(Dictionary<string, string> newLocalization)
        {
            var result = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kv in newLocalization)
            {
                string legacyLocalizationKey = ConvertToLegacyLocalizationKey(kv.Key);

                if (legacyLocalizationKey == null)
                    continue; // Dropping known language :'(

                result.Add(legacyLocalizationKey, kv.Value);
            }

            return result;
        }

        private void LoadWeapons<T>(WeaponType weaponType, string filename, List<WeaponBase> output) where T : WeaponBase
        {
            string fullFilename = Path.Combine(AppContext.BaseDirectory, "data", $"{filename}.json");

            if (File.Exists(fullFilename) == false)
            {
                IsFeatureEnabled = false;
                FeatureDisabledReason += $"Weapons data unavailable ('{filename}' missing)";
                return;
            }

            string weaponsContent = File.ReadAllText(fullFilename);
            IEnumerable<T> weapons = JsonConvert.DeserializeObject<IEnumerable<T>>(weaponsContent);

            foreach (WeaponBase weapon in weapons)
            {
                weapon.Type = weaponType;
                weapon.Name = ConvertToLegacyLocalization(weapon.Name);
                weapon.Description = ConvertToLegacyLocalization(weapon.Description);
            }

            output.AddRange(weapons);
        }

        private async Task<bool> LoadWeaponsInternal()
        {
            List<WeaponBase> allWeaponPrimitives = await LoadWeaponPrimitives();

            allWeapons = allWeaponPrimitives
                .Select(x => new WeaponViewModel(x))
                .ToDictionary(x => (x.Type, x.Id), x => x);

            var rootWeapons = new Dictionary<WeaponType, List<WeaponViewModel>>();

            foreach (WeaponBase primitive in allWeaponPrimitives)
            {
                WeaponViewModel weapon = allWeapons[(primitive.Type, (int)primitive.Id)];

                WeaponViewModel previous = null;
                if (primitive.ParentId > -1)
                    previous = allWeapons[(primitive.Type, primitive.ParentId)];

                if (previous != null)
                    weapon.SetParent(previous);
                else
                {
                    if (rootWeapons.TryGetValue(weapon.Type, out List<WeaponViewModel> weapons) == false)
                    {
                        weapons = new List<WeaponViewModel>();
                        rootWeapons.Add(weapon.Type, weapons);
                    }

                    weapons.Add(weapon);
                }
            }

            if (rootWeapons.Count == 0)
            {
                IsFeatureEnabled = false;
                FeatureDisabledReason = "No weapons available";
                return false;
            }

            WeaponTypes = rootWeapons
                .Select(kv => new WeaponTypeViewModel(kv.Key, kv.Value, this))
                .OrderBy(x => x.Type)
                .ToList();

            return true;
        }
    }
}
