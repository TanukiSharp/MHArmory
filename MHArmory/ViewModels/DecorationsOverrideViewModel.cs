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
using MHArmory.Services;
using MHArmory.Core.WPF;
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
        private readonly Func<IList<DecorationsSaveSlotInfo>, DecorationsSaveSlotInfo> saveSlotInfoSelector;

        public DecorationsOverrideViewModel(IList<IJewel> jewels, Func<IList<DecorationsSaveSlotInfo>, DecorationsSaveSlotInfo> saveSlotInfoSelector)
        {
            this.saveSlotInfoSelector = saveSlotInfoSelector;

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
                        JewelOverrideViewModel vm = Jewels.FirstOrDefault(x => Core.Localization.GetDefault(x.Name) == decoOverride.Key);
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

            var searchStatement = SearchStatement.Create(SearchText, GlobalData.Instance.Aliases);

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
                searchStatement = SearchStatement.Create(searchText, GlobalData.Instance.Aliases);

            jewelOverrideViewModel.ApplySearchText(searchStatement);
        }

        private async void OnImport(object paramter)
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

        private async Task ImportInternal()
        {
            ISaveDataService saveDataService = ServicesContainer.GetService<ISaveDataService>();

            IList<SaveDataInfo> saveDataInfoItems = saveDataService.GetSaveInfo();

            IList<Task<IList<DecorationsSaveSlotInfo>>> allTasks = saveDataInfoItems
                .Select(ReadSaveData)
                .ToList();

            await Task.WhenAll(allTasks);

            IList<DecorationsSaveSlotInfo> allSlots = allTasks
                .SelectMany(x => x.Result)
                .ToList();

            DecorationsSaveSlotInfo selected;

            if (allSlots.Count > 1)
            {
                selected = saveSlotInfoSelector(allSlots);
                if (selected == null)
                    return;
            }
            else
                selected = allSlots[0];

            MessageBox.Show("Save data import done.", "Import", MessageBoxButton.OK);

            ApplySaveDataDecorations(selected);
        }

        private void ApplySaveDataDecorations(DecorationsSaveSlotInfo saveSlotDecorations)
        {
            foreach (JewelOverrideViewModel child in Jewels)
                child.CanReportStateChange = false;

            try
            {
                foreach (JewelOverrideViewModel child in Jewels)
                {
                    child.IsOverriding = true;

                    string gameName = $"{Core.Localization.GetDefault(child.Name)} {child.SlotSize}";
                    JewelInfo foundGameJewel = MasterData.FindJewelInfoByName(gameName);

                    uint quantity = 0;
                    if (foundGameJewel.Name != null)
                    {
                        if (saveSlotDecorations.Decorations.TryGetValue(foundGameJewel.ItemId, out quantity))
                            child.IsOverriding = true;
                    }

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
            {
                var list = new List<DecorationsSaveSlotInfo>();

                foreach (DecorationsSaveSlotInfo info in reader.Read())
                {
                    info.SetSaveDataInfo(saveDataInfo);
                    list.Add(info);
                }

                return list;
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
