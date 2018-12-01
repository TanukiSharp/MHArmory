using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.ServiceContracts;
using MHWSaveUtils;
using Microsoft.Win32;

namespace MHArmory.Services
{
    public interface ISaveDataService
    {
        IList<SaveDataInfo> GetSaveInfo();
    }

    public class SaveDataService : ISaveDataService
    {
        public IList<SaveDataInfo> GetSaveInfo()
        {
            IMessageBoxService messageBoxService = ServicesContainer.GetService<IMessageBoxService>();

            IList<SaveDataInfo> saveDataInfoItems = FileSystemUtils.EnumerateSaveDataInfo().ToList();

            if (saveDataInfoItems.Count > 0)
                return saveDataInfoItems;

            var options = new MessageBoxServiceOptions
            {
                Title = "Save data not found",
                MessageBoxText = "Could not automatically find location of save data.\nPlease select it manually.",
                Buttons = MessageBoxButton.OK,
                Icon = MessageBoxImage.Warning
            };

            messageBoxService.Show(options);

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
                options = new MessageBoxServiceOptions
                {
                    Title = "Operation cancelled",
                    MessageBoxText = "Operation cancelled.",
                    Buttons = MessageBoxButton.OK,
                    Icon = MessageBoxImage.Warning
                };

                messageBoxService.Show(options);

                return null;
            }

            saveDataInfoItems.Add(new SaveDataInfo("<unknown>", dialog.FileName));

            return saveDataInfoItems;
        }
    }
}
