using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using MHArmory.Search.Contracts;
using MHArmory.Services;
using MHArmory.Core.ServiceContracts;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public class AdvancedSearchViewModel : ViewModelBase
    {
        private ArmorPieceTypesViewModel[] armorPieceTypes;
        public ArmorPieceTypesViewModel[] ArmorPieceTypes
        {
            get { return armorPieceTypes; }
            private set { SetValue(ref armorPieceTypes, value); }
        }

        public ICommand ExportToClipboardCommand { get; }
        public ICommand ImportFromClipboardCommand { get; }

        public AdvancedSearchViewModel()
        {
            ExportToClipboardCommand = new AnonymousCommand(OnExportToClipboard);
            ImportFromClipboardCommand = new AnonymousCommand(OnImportFromClipboard);
        }

        public void Update(ArmorPieceTypesViewModel[] armorPieceTypes)
        {
            ArmorPieceTypes = armorPieceTypes;
        }

        private void OnImportFromClipboard()
        {
            string value = Clipboard.GetText();
            DeserializeSelectedEquipments(value);
        }

        private void OnExportToClipboard()
        {
            string value = SerializeSelectedEquipments();
            Clipboard.SetText(value);
        }

        private void DeserializeSelectedEquipments(string value)
        {
            string error = null;

            try
            {
                List<Item> items = JsonConvert.DeserializeObject<List<Item>>(value);
                var eqpErrors = new List<string>();

                foreach (ArmorPieceTypesViewModel type in armorPieceTypes)
                {
                    Item item = items.FirstOrDefault(x => x.Type == (int)type.Type);

                    if (item.Selected == null)
                    {
                        error = "Invalid advanced search data";
                        break;
                    }

                    foreach (ISolverDataEquipmentModel eqp in type.Equipments)
                        eqp.IsSelected = false;

                    foreach (string eqpName in item.Selected)
                    {
                        ISolverDataEquipmentModel eqp = type.Equipments.FirstOrDefault(x => Core.Localization.GetDefault(x.Equipment.Name) == eqpName);

                        if (eqp == null)
                            eqpErrors.Add(eqpName);
                        else
                            eqp.IsSelected = true;
                    }
                }

                if (eqpErrors.Count > 0)
                {
                    string errMessage = string.Join("\n- ", eqpErrors);
                    error = $"The following equipment(s) could not be selected because unavailable:\n- {errMessage}";
                }
            }
            catch (Exception ex)
            {
                if (error == null)
                    error = ex.Message;
            }

            if (error != null)
            {
                IMessageBoxService service = ServicesContainer.GetService<IMessageBoxService>();
                service.Show(new MessageBoxServiceOptions
                {
                    Buttons = Core.ServiceContracts.MessageBoxButton.OK,
                    Icon = Core.ServiceContracts.MessageBoxImage.Warning,
                    MessageBoxText = error,
                    Title = "Error",
                });
            }
        }

        private string SerializeSelectedEquipments()
        {
            var results = new List<Item>();

            foreach (ArmorPieceTypesViewModel type in armorPieceTypes)
            {
                var item = new Item
                {
                    Type = (int)type.Type,
                    Selected = new List<string>()
                };

                foreach (ISolverDataEquipmentModel eqp in type.Equipments.Where(x => x.IsSelected))
                    item.Selected.Add(Core.Localization.GetDefault(eqp.Equipment.Name));

                results.Add(item);
            }

            return JsonConvert.SerializeObject(results, Formatting.None);
        }

        private struct Item
        {
            [JsonProperty("type")]
            public int Type { get; set; }
            [JsonProperty("selected")]
            public List<string> Selected { get; set; }
        }
    }
}
