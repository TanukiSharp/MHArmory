using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MHWSaveUtils;

namespace MHArmory.Services
{
    public interface ISaveDataAdvancedService
    {
        Task<EquipmentsSaveSlotInfo> GetEquipmentSaveSlot();
    }

    public class SaveDataAdvancedService : ISaveDataAdvancedService
    {
        public Task<EquipmentsSaveSlotInfo> GetEquipmentSaveSlot()
        {
            return Get(ReadEquipmentSaveData, ProvideEquipmentSaveSlotInfo);
        }

        private async Task<T> Get<T>(Func<SaveDataInfo, Task<IList<T>>> reader, Func<IList<T>, T> saveSlotInfoSelector) where T : BaseSaveSlotInfo
        {
            ISaveDataService saveDataService = ServicesContainer.GetService<ISaveDataService>();

            if (saveDataService == null)
                return null;

            IList<SaveDataInfo> saveDataInfoItems = saveDataService.GetSaveInfo();

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

        private async Task<IList<EquipmentsSaveSlotInfo>> ReadEquipmentSaveData(SaveDataInfo saveDataInfo)
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

        private EquipmentsSaveSlotInfo ProvideEquipmentSaveSlotInfo(IList<EquipmentsSaveSlotInfo> allSlots)
        {
            return ProvideSaveSlotInfo<EquipmentsSaveSlotInfo>(allSlots);
        }

        private T ProvideSaveSlotInfo<T>(IList<T> allSlots) where T : BaseSaveSlotInfo
        {
            var dialog = new SaveDataSlotSelectorWindow()
            {
                Owner = null
            };

            dialog.Initialize(allSlots);

            if (dialog.ShowDialog() != true)
                return null;

            return (T)dialog.SelectedSaveSlot;
        }
    }
}
