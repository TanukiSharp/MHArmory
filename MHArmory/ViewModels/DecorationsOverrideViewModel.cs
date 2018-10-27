using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MHArmory.Configurations;
using MHArmory.Core.DataStructures;
using MHWSaveUtils;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace MHArmory.ViewModels
{
    public enum DecorationOverrideVisibilityMode
    {
        /// <summary>
        /// Show all jewels.
        /// </summary>
        All,
        /// <summary>
        /// Show only jewels where override checkbox is checked or count is greater than zero.
        /// </summary>
        Modified,
        /// <summary>
        /// Show only jewels where override checkbox is unchecked and count is zero.
        /// </summary>
        Unmodified
    }

    public class DecorationsOverrideViewModel : ViewModelBase
    {
        public bool HasChanged { get; private set; }

        private IEnumerable<JewelOverrideViewModel> jewels;
        public IEnumerable<JewelOverrideViewModel> Jewels
        {
            get { return jewels; }
            set { SetValue(ref jewels, value); }
        }

        private DecorationOverrideVisibilityMode visibilityMode = DecorationOverrideVisibilityMode.All;

        public bool VisibilityModeAll
        {
            set
            {
                if (value && visibilityMode != DecorationOverrideVisibilityMode.All)
                {
                    visibilityMode = DecorationOverrideVisibilityMode.All;
                    ComputeVisibility();
                }
            }
        }

        public bool VisibilityModeModified
        {
            set
            {
                if (value && visibilityMode != DecorationOverrideVisibilityMode.Modified)
                {
                    visibilityMode = DecorationOverrideVisibilityMode.Modified;
                    ComputeVisibility();
                }
            }
        }

        public bool VisibilityModeUnmodified
        {
            set
            {
                if (value && visibilityMode != DecorationOverrideVisibilityMode.Unmodified)
                {
                    visibilityMode = DecorationOverrideVisibilityMode.Unmodified;
                    ComputeVisibility();
                }
            }
        }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (SetValue(ref searchText, value))
                {
                    if (Jewels != null)
                        ComputeVisibility();
                }
            }
        }

        public ICommand CancelCommand { get; }
        public ICommand OpenIntegratedHelpCommand { get; }
        public ICommand ImportCommand { get; }

        private readonly bool isLoadingConfiguration;

        public DecorationsOverrideViewModel(IList<IJewel> jewels)
        {
            Jewels = jewels
                .Select(x => new JewelOverrideViewModel(this, x, 0))
                .ToList();

            Dictionary<string, DecorationOverrideConfigurationItem> decorationOverrides = GlobalData.Instance.Configuration.InParameters?.DecorationOverride?.Items;

            CancelCommand = new AnonymousCommand(OnCancel);
            OpenIntegratedHelpCommand = new AnonymousCommand(OnOpenIntegratedHelp);
            ImportCommand = new AnonymousCommand(OnImport);

            if (decorationOverrides != null)
            {
                isLoadingConfiguration = true;

                try
                {
                    foreach (KeyValuePair<string, DecorationOverrideConfigurationItem> decoOverride in decorationOverrides)
                    {
                        JewelOverrideViewModel vm = Jewels.FirstOrDefault(x => x.Name == decoOverride.Key);
                        if (vm != null)
                        {
                            vm.IsOverriding = decoOverride.Value.IsOverriding;
                            vm.Count = decoOverride.Value.Count;
                        }
                    }
                }
                finally
                {
                    isLoadingConfiguration = false;
                }
            }
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

        private void OnOpenIntegratedHelp(object parameter)
        {
            WindowManager.Show<HelpWindow>(parameter);
        }

        internal void StateChanged()
        {
            if (isLoadingConfiguration)
                return;

            HasChanged = true;
        }

        private void ComputeVisibility()
        {
            if (isLoadingConfiguration)
                return;

            var searchStatement = SearchStatement.Create(SearchText);

            foreach (JewelOverrideViewModel vm in Jewels)
                ComputeVisibility(vm, searchStatement);
        }

        internal void ComputeVisibility(JewelOverrideViewModel jewelOverrideViewModel, SearchStatement searchStatement = null)
        {
            if (visibilityMode == DecorationOverrideVisibilityMode.Modified)
            {
                if (jewelOverrideViewModel.IsOverriding == false && jewelOverrideViewModel.Count == 0)
                {
                    jewelOverrideViewModel.IsVisible = false;
                    return;
                }
            }
            else if (visibilityMode == DecorationOverrideVisibilityMode.Unmodified)
            {
                if (jewelOverrideViewModel.IsOverriding || jewelOverrideViewModel.Count > 0)
                {
                    jewelOverrideViewModel.IsVisible = false;
                    return;
                }
            }

            if (searchStatement == null)
                searchStatement = SearchStatement.Create(searchText);

            jewelOverrideViewModel.ApplySearchText(searchStatement);
        }

        private async void OnImport(object paramter)
        {
            IList<SaveDataInfo> saveDataInfoItems = FileSystemUtils.EnumerateSaveDataInfo().Take(0).ToList();

            if (saveDataInfoItems.Count == 0)
            {
                MessageBox.Show("Could not automatically find location of save data.\nPlease select it manually.", "Save data not found", MessageBoxButton.OK, MessageBoxImage.Warning);

                var dialog = new OpenFileDialog
                {
                    AddExtension = false,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    FileName = FileSystemUtils.GameSaveDataFilename,
                    Filter = $"Save data|{FileSystemUtils.GameSaveDataFilename}|All files (*.*)|*.*",
                    Multiselect = false,
                    ShowReadOnly = true,
                    Title = "Select Monster Hunter: World save data file"
                };
                if (dialog.ShowDialog() != true)
                {
                    MessageBox.Show("Operation cancelled.", "Operation cancelled", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                saveDataInfoItems.Add(new SaveDataInfo("<unknown>", dialog.FileName));
            }

            IList<IList<DecorationsSaveSlotInfo>> decorationsPerSave = null;

            IList<Task<IList<DecorationsSaveSlotInfo>>> allTasks = saveDataInfoItems.Select(ReadSaveData).ToList();
            await Task.WhenAll(allTasks);
            decorationsPerSave = allTasks.Select(x => x.Result).ToList();

            int saveDataIndex = 0;
            int saveSlotIndex = 0;

            if (saveDataInfoItems.Count > 1)
            {
                // Show the save data slot selector window
            }

            ApplySaveDataDecorations(decorationsPerSave[saveDataIndex][saveSlotIndex]);
        }

        private void ApplySaveDataDecorations(DecorationsSaveSlotInfo saveSlotDecorations)
        {
            IList<GameJewel> gameJewels = LoadGameJewels();
            if (gameJewels == null)
                return;

            foreach (JewelOverrideViewModel child in Jewels)
                child.CanReportStateChange = false;

            try
            {
                foreach (JewelOverrideViewModel child in Jewels)
                {
                    child.IsOverriding = true;

                    string gameName = $"{child.Name} {child.SlotSize}";
                    GameJewel foundGameJewel = gameJewels.FirstOrDefault(x => x.Name == gameName);

                    uint quantity = 0;
                    if (foundGameJewel != null)
                        saveSlotDecorations.Decorations.TryGetValue(foundGameJewel.Id, out quantity);

                    child.Count = (int)quantity;
                }
            }
            finally
            {
                foreach (JewelOverrideViewModel child in Jewels)
                    child.CanReportStateChange = true;
            }

            ComputeVisibility();

            HasChanged = true;
        }

        private async Task<IList<DecorationsSaveSlotInfo>> ReadSaveData(SaveDataInfo saveDataInfo)
        {
            var ms = new MemoryStream();

            using (Stream inputStream = File.OpenRead(saveDataInfo.SaveDataFullFilename))
            {
                await Crypto.DecryptAsync(inputStream, ms, CancellationToken.None);
            }

            using (var reader = new DecorationsReader(ms))
                return reader.Read().ToList();
        }

        private static IList<GameJewel> LoadGameJewels()
        {
            try
            {
                string dataPath = Path.Combine(AppContext.BaseDirectory, "data");
                string gameJewelsContent = File.ReadAllText(Path.Combine(dataPath, "gameJewels.json"));
                return JsonConvert.DeserializeObject<IList<GameJewel>>(gameJewelsContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured when trying to load game jewels information.\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public class GameJewel
        {
            [JsonProperty("id")]
            public uint Id { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}
