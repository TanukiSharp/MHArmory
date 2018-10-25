using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHWSaveUtils;

namespace MHArmory.ViewModels
{
    public class SaveDataAccountViewModel : ViewModelBase
    {
        public int Index { get; }
        public string UserId { get; }

        public SaveDataAccountViewModel(int index, SaveDataInfo saveDataInfo)
        {
            Index = index;
            UserId = saveDataInfo.UserId;

        }
    }

    public class SaveDataSlotSelectorViewModel : ViewModelBase
    {
        public int SelectedAccountIndex { get; set; }
        public int SelectedSaveSlotIndex { get; set; }

        public SaveDataSlotSelectorViewModel(IList<SaveDataInfo> saveDataInfo)
        {
        }
    }
}
