using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core.WPF;
using MHArmory.Search.Contracts;
using Newtonsoft.Json;

namespace MHArmory.Search.Testing
{
    public class SampleViewModel : ViewModelBase
    {
        private string textValue;
        public string TextValue
        {
            get { return textValue; }
            set
            {
                if (SetValue(ref textValue, value))
                    Console.WriteLine($"{nameof(TextValue)} changed to {textValue}");
            }
        }

        public int numericValue;
        public int NumericValue
        {
            get { return numericValue; }
            set
            {
                if (SetValue(ref numericValue, value))
                    Console.WriteLine($"{nameof(NumericValue)} changed to {numericValue}");
            }
        }

        private int anotherNumericValue;
        public int AnotherNumericValue
        {
            get { return anotherNumericValue; }
            set { SetValue(ref anotherNumericValue, value); } // Here we don't care if the value changed.
        }

        public ICommand ActionCommand { get; } // Commands are usually set only once.

        private readonly ISolver solver;

        public SampleViewModel(ISolver solver)
        {
            this.solver = solver;
            ActionCommand = new AnonymousCommand(OnAction);

            LoadSettings();
        }

        private const string SettingsFilename = "sample-extension-settings.json";

        private void LoadSettings()
        {
            string filename = Path.Combine(AppContext.BaseDirectory, SettingsFilename);

            if (File.Exists(filename) == false)
                return;

            string jsonContent = File.ReadAllText(filename);

            SampleModel model;

            try
            {
                model = JsonConvert.DeserializeObject<SampleModel>(jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return;
            }

            TextValue = model.Text;
            NumericValue = model.Numeric;
            AnotherNumericValue = model.AnotherNumeric;
        }

        private void OnAction()
        {
            Console.WriteLine("Action executed");
        }

        public void OnClose()
        {
            string jsonContent = JsonConvert.SerializeObject(new SampleModel
            {
                Text = TextValue,
                Numeric = NumericValue,
                AnotherNumeric = AnotherNumericValue
            }, Formatting.Indented);

            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, SettingsFilename), jsonContent);
        }
    }
}
