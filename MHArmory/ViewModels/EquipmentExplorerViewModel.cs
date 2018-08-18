using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class EquipmentExplorerEquipmentViewModel : ViewModelBase
    {
        public IEquipment Equipment { get; }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        public EquipmentExplorerEquipmentViewModel(IEquipment equipment)
        {
            Equipment = equipment;
        }
    }

    public class EquipmentExplorerViewModel : ViewModelBase
    {
        public IList<EquipmentExplorerEquipmentViewModel> Equipments { get; private set; }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (SetValue(ref searchText, value))
                    OnSearchTextChanged();
            }
        }

        private void OnSearchTextChanged()
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (EquipmentExplorerEquipmentViewModel x in Equipments)
                    x.IsVisible = true;
            }
            else
            {
                var searchStatement = SearchStatement.Create(searchText);

                foreach (EquipmentExplorerEquipmentViewModel x in Equipments)
                    x.IsVisible = searchStatement.IsMatching(x.Equipment.Name);
            }
        }

        public void CreateItems()
        {
            if (Equipments != null)
                return;

            IList<IArmorPiece> heads = GlobalData.Instance.Heads;
            IList<IArmorPiece> chests = GlobalData.Instance.Chests;
            IList<IArmorPiece> gloves = GlobalData.Instance.Gloves;
            IList<IArmorPiece> waists = GlobalData.Instance.Waists;
            IList<IArmorPiece> legs = GlobalData.Instance.Legs;
            IList<ICharmLevel> charms = GlobalData.Instance.Charms;

            Equipments = heads
                .Concat<IEquipment>(chests)
                .Concat<IEquipment>(gloves)
                .Concat<IEquipment>(waists)
                .Concat<IEquipment>(legs)
                .Concat<IEquipment>(charms)
                .Select(x => new EquipmentExplorerEquipmentViewModel(x))
                .ToList();
        }
    }
}
