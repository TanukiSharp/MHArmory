using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.ViewModels
{
    public class EquipmentExplorerViewModel : ViewModelBase
    {
        private IList<IEquipment> allEquipments;

        public ObservableCollection<IEquipment> Equipments { get; } = new ObservableCollection<IEquipment>();

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
            Equipments.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (IEquipment x in allEquipments)
                    Equipments.Add(x);
            }
            else
            {
                var searchStatement = SearchStatement.Create(searchText);

                foreach (IEquipment x in allEquipments)
                {
                    bool isVisible = searchStatement.IsMatching(x.Name);
                    if (isVisible)
                        Equipments.Add(x);
                }
            }
        }

        public void CreateItems()
        {
            if (allEquipments != null)
                return;

            IList<IArmorPiece> heads = GlobalData.Instance.Heads;
            IList<IArmorPiece> chests = GlobalData.Instance.Chests;
            IList<IArmorPiece> gloves = GlobalData.Instance.Gloves;
            IList<IArmorPiece> waists = GlobalData.Instance.Waists;
            IList<IArmorPiece> legs = GlobalData.Instance.Legs;
            IList<ICharmLevel> charms = GlobalData.Instance.Charms;

            allEquipments = heads
                .Concat<IEquipment>(chests)
                .Concat<IEquipment>(gloves)
                .Concat<IEquipment>(waists)
                .Concat<IEquipment>(legs)
                .Concat<IEquipment>(charms)
                .ToList();

            foreach (IEquipment x in allEquipments)
                Equipments.Add(x);
        }
    }
}
