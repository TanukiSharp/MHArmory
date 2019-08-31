using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MHWSaveUtils;
using Microsoft.Win32;

namespace MHArmory
{
    public static class SaveDataUtils
    {
        public static IList<SaveDataInfo> GetSaveInfo()
        {
            IList<SaveDataInfo> saveDataInfoItems = FileSystemUtils.EnumerateSaveDataInfo().ToList();

            if (saveDataInfoItems.Count > 0)
                return saveDataInfoItems;

            MessageBox.Show(
                "Could not automatically find location of save data.\nPlease select it manually.",
                "Save data not found",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );

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
                MessageBox.Show(
                    "Operation cancelled.",
                    "Operation cancelled",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return null;
            }

            saveDataInfoItems.Add(new SaveDataInfo("<unknown>", dialog.FileName));

            return saveDataInfoItems;
        }

        public static Task<EquipmentSaveSlotInfo> GetEquipmentSaveSlot(Func<IList<EquipmentSaveSlotInfo>, EquipmentSaveSlotInfo> saveSlotInfoSelector)
        {
            return Get(ReadEquipmentSaveData, saveSlotInfoSelector);
        }

        public static Task<DecorationsSaveSlotInfo> GetDecorationsSaveSlot(Func<IList<DecorationsSaveSlotInfo>, DecorationsSaveSlotInfo> saveSlotInfoSelector)
        {
            return Get(ReadDecorationsSaveData, saveSlotInfoSelector);
        }

        public static Func<IList<T>, T> CreateSaveSlotSelector<T>(Window ownerWindow) where T : SaveSlotInfoBase
        {
            return slots =>
            {
                var dialog = new SaveDataSlotSelectorWindow()
                {
                    Owner = ownerWindow
                };

                dialog.Initialize(slots);

                if (dialog.ShowDialog() != true)
                    return null;

                return (T)dialog.SelectedSaveSlot;
            };
        }

        private static async Task<T> Get<T>(Func<SaveDataInfo, Task<IList<T>>> reader, Func<IList<T>, T> saveSlotInfoSelector) where T : SaveSlotInfoBase
        {
            IList<SaveDataInfo> saveDataInfoItems = GetSaveInfo();

            if (saveDataInfoItems == null)
                return null;

            IList<Task<IList<T>>> allTasks = saveDataInfoItems
                .Select(reader)
                .ToList();

            await Task.WhenAll(allTasks);

            IList<T> allSlots = allTasks
                .SelectMany(x => x.Result)
                .ToList();

            T selected;

            if (allSlots.Count > 1)
            {
                selected = saveSlotInfoSelector(allSlots);
                if (selected == null)
                    return null;
            }
            else
                selected = allSlots[0];

            MessageBox.Show("Save data import done.", "Import", MessageBoxButton.OK);

            return selected;
        }

        private static async Task<IList<EquipmentSaveSlotInfo>> ReadEquipmentSaveData(SaveDataInfo saveDataInfo)
        {
            var ms = new MemoryStream();

            using (Stream inputStream = File.OpenRead(saveDataInfo.SaveDataFullFilename))
            {
                await Crypto.DecryptAsync(inputStream, ms, CancellationToken.None);
            }

            using (var reader = new EquipmentReader(ms))
            {
                var list = new List<EquipmentSaveSlotInfo>();

                foreach (EquipmentSaveSlotInfo info in reader.Read())
                {
                    info.SetSaveDataInfo(saveDataInfo);
                    list.Add(info);
                }

                return list;
            }
        }

        private static async Task<IList<DecorationsSaveSlotInfo>> ReadDecorationsSaveData(SaveDataInfo saveDataInfo)
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
    }
}
