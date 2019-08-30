using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Configurations;
using MHArmory.Core;
using MHArmory.Core.DataStructures;
using MHArmory.Services;
using MHArmory.Core.WPF;
using MHWSaveUtils;
using Newtonsoft.Json;

namespace MHArmory.ViewModels
{
    public enum EquipmentOverrideVisibilityMode
    {
        /// <summary>
        /// Shows all sets.
        /// </summary>
        All,
        /// <summary>
        /// Show all sets where all armor pieces are owned.
        /// </summary>
        AllPossessed,
        /// <summary>
        /// Show only sets where at least one piece is owned.
        /// </summary>
        SomePossessed,
        /// <summary>
        /// Show only sets where at least one piece is not owned.
        /// </summary>
        SomeUnpossessed,
        /// <summary>
        /// Show only sets where no armor piece is owned.
        /// </summary>
        AllUnpossessed
    }

    public class CharmGroupViewModel : EquipmentGroupViewModel
    {
        public CharmGroupViewModel(EquipmentOverrideViewModel parent, IEnumerable<EquipmentViewModel> equipments)
            : base(parent, equipments)
        {
            OrderedEquipments = MakeCharms(Equipments).ToList();
            Name = ((ICharmLevel)Equipments[0].Equipment).Charm.Name;
        }

        private static IEnumerable<EquipmentViewModel> MakeCharms(IEnumerable<EquipmentViewModel> charms)
        {
            int total = 5;

            foreach (EquipmentViewModel charm in charms)
            {
                yield return charm;
                total--;
            }

            while (total-- > 0)
                yield return null;
        }
    }

    public class ArmorGroupViewModel : EquipmentGroupViewModel
    {
        public ArmorGroupViewModel(EquipmentOverrideViewModel parent, IEnumerable<ArmorPieceViewModel> equipments)
            : base(parent, equipments)
        {
            OrderedEquipments = MakeArmorPieces(Equipments).ToList();
            Name = Localization.AvailableLanguageCodes.ToDictionary(
                kv1 => kv1.Key,
                kv2 => FindGroupName(Equipments, kv2.Key)
            );
        }

        private static IEnumerable<EquipmentViewModel> MakeArmorPieces(IEnumerable<EquipmentViewModel> equipments)
        {
            yield return equipments.FirstOrDefault(x => x.Type == Core.DataStructures.EquipmentType.Head);
            yield return equipments.FirstOrDefault(x => x.Type == Core.DataStructures.EquipmentType.Chest);
            yield return equipments.FirstOrDefault(x => x.Type == Core.DataStructures.EquipmentType.Gloves);
            yield return equipments.FirstOrDefault(x => x.Type == Core.DataStructures.EquipmentType.Waist);
            yield return equipments.FirstOrDefault(x => x.Type == Core.DataStructures.EquipmentType.Legs);
        }

        public static string FindGroupName(IEnumerable<IEquipment> equipments, string language)
        {
            if (equipments == null)
                return null;

            string baseName = null;
            int firstPartMinLength = 0;
            int lastPartMinLength = 0;

            foreach (IEquipment eqp in equipments)
            {
                if (eqp == null)
                    continue;

                if (baseName == null)
                {
                    baseName = eqp.Name[language];
                    firstPartMinLength = baseName.Length;
                    lastPartMinLength = baseName.Length;
                    continue;
                }

                int c;
                string name = eqp.Name[language];

                for (c = 0; c < name.Length && c < baseName.Length; c++)
                {
                    if (name[c] != baseName[c])
                        break;
                }

                if (c < firstPartMinLength)
                    firstPartMinLength = c;

                for (c = 0; c < name.Length && c < baseName.Length; c++)
                {
                    if (name[name.Length - c - 1] != baseName[baseName.Length - c - 1])
                        break;
                }

                if (c < lastPartMinLength)
                    lastPartMinLength = c;
            }

            if (firstPartMinLength == 0)
                return equipments.First().Name[language]; // FIXME: Quick and dirty fallback, need to add sets game data for proper fix

            if (lastPartMinLength == 0 || firstPartMinLength == lastPartMinLength)
            {
                if (firstPartMinLength == baseName.Length)
                    return baseName;

                return baseName.Substring(0, firstPartMinLength).Trim();
            }

            string firstPart = baseName.Substring(0, firstPartMinLength).Trim();
            string lastPart = baseName.Substring(baseName.Length - lastPartMinLength).Trim();

            return $"{firstPart} {lastPart}";
        }
    }

    public abstract class EquipmentGroupViewModel : ViewModelBase
    {
        public Dictionary<string, string> Name { get; protected set; }

        public IList<EquipmentViewModel> OrderedEquipments { get; protected set; }
        public IList<EquipmentViewModel> Equipments { get; }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        public bool PossessNone
        {
            get
            {
                return Equipments.All(x => x.IsPossessed == false);
            }
        }

        public bool PossessAll
        {
            get
            {
                return Equipments.All(x => x.IsPossessed);
            }
        }

        public bool PossessAny
        {
            get
            {
                return Equipments.Any(x => x.IsPossessed);
            }
        }

        public ICommand ToggleAllCommand { get; }

        private readonly EquipmentOverrideViewModel parent;

        protected EquipmentGroupViewModel(EquipmentOverrideViewModel parent, IEnumerable<EquipmentViewModel> equipments)
        {
            this.parent = parent;

            Equipments = equipments.Where(x => x != null).ToList();

            ToggleAllCommand = new AnonymousCommand(OnToggleAll);
        }

        public void ApplySearchText(SearchStatement searchStatement)
        {
            if (searchStatement == null || searchStatement.IsEmpty)
            {
                IsVisible = true;
                return;
            }

            IsVisible =
                searchStatement.IsMatching(Localization.Get(Name)) ||
                Equipments.Any(x => searchStatement.IsMatching(Localization.Get(x.Name)));
        }

        private void OnToggleAll()
        {
            bool allChecked = Equipments.All(x => x.IsPossessed);

            foreach (EquipmentViewModel equipment in Equipments)
                equipment.IsPossessed = allChecked == false;
        }
    }

    public enum EquipmentViewCategory
    {
        LowRankArmors,
        HighRankArmors,
        Charms
    }

    public class EquipmentOverrideViewModel : ViewModelBase
    {
        private readonly RootViewModel rootViewModel;

        private IList<ArmorGroupViewModel> lowRankArmors;
        public IList<ArmorGroupViewModel> LowRankArmors
        {
            get { return lowRankArmors; }
            private set { SetValue(ref lowRankArmors, value); }
        }

        private IList<ArmorGroupViewModel> highRankArmors;
        public IList<ArmorGroupViewModel> HighRankArmors
        {
            get { return highRankArmors; }
            private set { SetValue(ref highRankArmors, value); }
        }

        private IList<CharmGroupViewModel> charms;
        public IList<CharmGroupViewModel> Charms
        {
            get { return charms; }
            private set { SetValue(ref charms, value); }
        }

        private IList<EquipmentGroupViewModel> allEquipments;
        public IList<EquipmentGroupViewModel> AllEquipments
        {
            get { return allEquipments; }
            private set { SetValue(ref allEquipments, value); }
        }

        public ICommand SelectAllCommand { get; }
        public ICommand UnselectAllCommand { get; }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (SetValue(ref searchText, value))
                    ComputeVisibility();
            }
        }

        private string status;
        public string Status
        {
            get { return status; }
            private set { SetValue(ref status, value); }
        }

        private EquipmentOverrideVisibilityMode visibilityMode = EquipmentOverrideVisibilityMode.All;
        public EquipmentOverrideVisibilityMode VisibilityMode
        {
            get { return visibilityMode; }
            set
            {
                if (SetValue(ref visibilityMode, value))
                    ComputeVisibility();
            }
        }

        public ICommand ImportCommand { get; }
        public ICommand OpenIntegratedHelpCommand { get; }
        public ICommand CancelCommand { get; }

        private Func<IList<EquipmentsSaveSlotInfo>, EquipmentsSaveSlotInfo> saveSlotInfoSelector;

        public EquipmentOverrideViewModel(RootViewModel rootViewModel)
        {
            this.rootViewModel = rootViewModel;

            SelectAllCommand = new AnonymousCommand(OnSelectAll);
            UnselectAllCommand = new AnonymousCommand(OnUnselectAll);

            ImportCommand = new AnonymousCommand(OnImport);
            OpenIntegratedHelpCommand = new AnonymousCommand(OnOpenIntegratedHelp);

            CancelCommand = new AnonymousCommand(OnCancel);
        }

        private void OnSelectAll()
        {
            foreach (EquipmentGroupViewModel group in AllEquipments)
            {
                foreach (EquipmentViewModel eqp in group.Equipments)
                    eqp.IsPossessed = true;
            }
        }

        private void OnUnselectAll()
        {
            foreach (EquipmentGroupViewModel group in AllEquipments)
            {
                foreach (EquipmentViewModel eqp in group.Equipments)
                    eqp.IsPossessed = false;
            }
        }

        public void SetSaveSelector(Func<IList<EquipmentsSaveSlotInfo>, EquipmentsSaveSlotInfo> saveSlotInfoSelector)
        {
            this.saveSlotInfoSelector = saveSlotInfoSelector;
        }

        private async void OnImport(object parameter)
        {
            ((AnonymousCommand)ImportCommand).IsEnabled = false;

            try
            {
                await ImportInternal();
            }
            finally
            {
                ((AnonymousCommand)ImportCommand).IsEnabled = true;
            }
        }

        private IList<GameEquipment> gameEquipments;

        private async Task ImportInternal()
        {
            if (gameEquipments == null)
            {
                gameEquipments = LoadGameEquipments();
                if (gameEquipments == null)
                    return;
            }

            ISaveDataService saveDataService = ServicesContainer.GetService<ISaveDataService>();

            if (saveDataService == null)
                return;

            IList<SaveDataInfo> saveDataInfoItems = saveDataService.GetSaveInfo();

            if (saveDataInfoItems == null)
                return;

            IList<Task<IList<EquipmentsSaveSlotInfo>>> allTasks = saveDataInfoItems
                .Select(ReadSaveData)
                .ToList();

            await Task.WhenAll(allTasks);

            IList<EquipmentsSaveSlotInfo> allSlots = allTasks
                .SelectMany(x => x.Result)
                .ToList();

            EquipmentsSaveSlotInfo selected;

            if (allSlots.Count > 1)
            {
                selected = saveSlotInfoSelector(allSlots);
                if (selected == null)
                    return;
            }
            else
                selected = allSlots[0];

            System.Windows.MessageBox.Show("Save data import done.", "Import", System.Windows.MessageBoxButton.OK);

            ApplySaveDataEquipments(selected);
        }

        private async Task<IList<EquipmentsSaveSlotInfo>> ReadSaveData(SaveDataInfo saveDataInfo)
        {
            var ms = new MemoryStream();

            using (Stream inputStream = File.OpenRead(saveDataInfo.SaveDataFullFilename))
            {
                await Crypto.DecryptAsync(inputStream, ms, CancellationToken.None);
            }

            using (var reader = new EquipmentsReader(ms))
            {
                var list = new List<EquipmentsSaveSlotInfo>();

                foreach (EquipmentsSaveSlotInfo info in reader.Read())
                {
                    info.SetSaveDataInfo(saveDataInfo);
                    list.Add(info);
                }

                return list;
            }
        }

        private static bool IsMatch(Equipment saveDataEquipment, GameEquipment masterDataEquipment)
        {
            if (saveDataEquipment.ClassId != masterDataEquipment.Id)
                return false;

            if (saveDataEquipment.Type == MHWSaveUtils.EquipmentType.Armor &&
                saveDataEquipment.ArmorPieceType == (ArmorPieceType)(masterDataEquipment.Type - 1))
                return true;

            if (saveDataEquipment.Type == MHWSaveUtils.EquipmentType.Charm &&
                (Core.DataStructures.EquipmentType)masterDataEquipment.Type == Core.DataStructures.EquipmentType.Charm)
                return true;

            return false;
        }

        private void ApplySaveDataEquipments(EquipmentsSaveSlotInfo saveSlotEquipments)
        {
            foreach (EquipmentGroupViewModel group in AllEquipments)
            {
                foreach (EquipmentViewModel equipment in group.Equipments)
                {
                    equipment.IsPossessed = false;

                    GameEquipment foundGameEquipmentFromMasterData = gameEquipments.FirstOrDefault(x => x.Name == Localization.Get(equipment.Name));

                    if (foundGameEquipmentFromMasterData == null)
                        Console.WriteLine($"Missing equipment from master data: {equipment.Name}");
                    else
                    {
                        Equipment n = saveSlotEquipments.Equipments
                            .FirstOrDefault(x => IsMatch(x, foundGameEquipmentFromMasterData));

                        if (n != null)
                            equipment.IsPossessed = true;
                    }
                }
            }

            ComputeVisibility();
        }

        private static IList<GameEquipment> LoadGameEquipments()
        {
            try
            {
                string dataPath = Path.Combine(AppContext.BaseDirectory, "data");
                string gameEquipmentsContent = File.ReadAllText(Path.Combine(dataPath, "gameEquipments.json"));
                return JsonConvert.DeserializeObject<IList<GameEquipment>>(gameEquipmentsContent);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"An error occured when trying to load game equipments information.\n\n{ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
                return null;
            }
        }

        public class GameEquipment
        {
            [JsonProperty("id")]
            public uint Id { get; set; }
            [JsonProperty("type")]
            public uint Type { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        private void OnOpenIntegratedHelp(object parameter)
        {
            WindowManager.Show<HelpWindow>(parameter);
        }

        private void OnCancel(object parameter)
        {
            if (parameter is CancellationCommandArgument cancellable)
            {
                if (string.IsNullOrWhiteSpace(SearchText) == false)
                {
                    SearchText = string.Empty;
                    cancellable.IsCancelled = true;
                }
            }
        }

        public void ComputeVisibility()
        {
            var searchStatement = SearchStatement.Create(SearchText, GlobalData.Instance.Aliases);

            foreach (EquipmentGroupViewModel vm in AllEquipments)
                ComputeVisibility(vm, searchStatement);

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            Status = $"{AllEquipments.Count(x => x.IsVisible)} sets";
        }

        private void ComputeVisibility(EquipmentGroupViewModel group, SearchStatement searchStatement)
        {
            if (visibilityMode == EquipmentOverrideVisibilityMode.AllPossessed)
            {
                if (group.PossessAll == false)
                {
                    group.IsVisible = false;
                    return;
                }
            }
            else if (visibilityMode == EquipmentOverrideVisibilityMode.SomePossessed)
            {
                if (group.PossessAny == false)
                {
                    group.IsVisible = false;
                    return;
                }
            }
            else if (visibilityMode == EquipmentOverrideVisibilityMode.SomeUnpossessed)
            {
                if (group.PossessAll || group.PossessNone)
                {
                    group.IsVisible = false;
                    return;
                }
            }
            else if (visibilityMode == EquipmentOverrideVisibilityMode.AllUnpossessed)
            {
                if (group.PossessAny)
                {
                    group.IsVisible = false;
                    return;
                }
            }

            if (searchStatement == null)
                searchStatement = SearchStatement.Create(searchText, GlobalData.Instance.Aliases);

            group.ApplySearchText(searchStatement);
        }

        internal void NotifyDataLoaded()
        {
            LowRankArmors = rootViewModel.AllEquipments
                .Where(x => x.Rarity <= 4)
                .Where(x => x.Type != Core.DataStructures.EquipmentType.Weapon && x.Type != Core.DataStructures.EquipmentType.Charm)
                .Cast<ArmorPieceViewModel>()
                .GroupBy(x => x.Id)
                .OrderBy(x => x.Key)
                .Select(x => new ArmorGroupViewModel(this, x))
                .ToList();

            HighRankArmors = rootViewModel.AllEquipments
                .Where(x => x.Rarity > 4)
                .Where(x => x.Type != Core.DataStructures.EquipmentType.Weapon && x.Type != Core.DataStructures.EquipmentType.Charm)
                .Cast<ArmorPieceViewModel>()
                .GroupBy(x => x.Id)
                .OrderBy(x => x.Key)
                .Select(x => new ArmorGroupViewModel(this, x))
                .ToList();

            Charms = rootViewModel.AllEquipments
                .Where(x => x.Type == Core.DataStructures.EquipmentType.Charm)
                .GroupBy(x => ((ICharmLevel)x.Equipment).Charm.Id)
                .OrderBy(x => x.Key)
                .Select(x => new CharmGroupViewModel(this, x))
                .ToList();

            AllEquipments = LowRankArmors
                .Cast<EquipmentGroupViewModel>()
                .Concat(HighRankArmors)
                .Concat(Charms)
                .ToList();

            LoadConfiguration();

            UpdateStatus();
        }

        private static int GroupOperator(EquipmentViewModel eqp)
        {
            if (eqp.Type != Core.DataStructures.EquipmentType.Charm)
                return eqp.Id;

            return ((ICharmLevel)eqp.Equipment).Charm.Id + 10000;
        }

        private void LoadConfiguration()
        {
            EquipmentOverrideConfigurationV2 configuration = GlobalData.Instance.Configuration.InParameters.EquipmentOverride;

            if (configuration.IsStoringPossessed)
            {
                foreach (EquipmentGroupViewModel group in AllEquipments)
                {
                    foreach (EquipmentViewModel equipment in group.Equipments)
                        equipment.IsPossessed = configuration.Items.Contains(Localization.GetDefault(equipment.Name));
                }
            }
            else
            {
                foreach (EquipmentGroupViewModel group in AllEquipments)
                {
                    foreach (EquipmentViewModel equipment in group.Equipments)
                        equipment.IsPossessed = configuration.Items.Contains(Localization.GetDefault(equipment.Name)) == false;
                }
            }
        }

        public void SaveConfiguration()
        {
            int total = 0;
            int totalPossessed = 0;

            foreach (EquipmentGroupViewModel group in AllEquipments)
            {
                foreach (EquipmentViewModel equipment in group.Equipments)
                {
                    total++;
                    if (equipment.IsPossessed)
                        totalPossessed++;
                }
            }

            EquipmentOverrideConfigurationV2 configuration = GlobalData.Instance.Configuration.InParameters.EquipmentOverride;

            configuration.UseOverride = rootViewModel.InParameters.UseEquipmentOverride;
            configuration.Items.Clear();

            if (totalPossessed < (total - totalPossessed))
            {
                // save possessed ones
                configuration.IsStoringPossessed = true;

                foreach (EquipmentGroupViewModel group in AllEquipments)
                {
                    foreach (EquipmentViewModel equipment in group.Equipments.Where(x => x.IsPossessed))
                        configuration.Items.Add(Localization.GetDefault(equipment.Name));
                }
            }
            else
            {
                // save not possessed ones
                configuration.IsStoringPossessed = false;

                foreach (EquipmentGroupViewModel group in AllEquipments)
                {
                    foreach (EquipmentViewModel equipment in group.Equipments.Where(x => x.IsPossessed == false))
                        configuration.Items.Add(Localization.GetDefault(equipment.Name));
                }
            }

            ConfigurationManager.Save(GlobalData.Instance.Configuration);
        }
    }
}
