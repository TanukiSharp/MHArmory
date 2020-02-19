using MHArmory.Core.WPF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.Search.Default
{
    internal class SolverDataViewModel : ViewModelBase
    {
        private class Configuration
        {
            [JsonProperty("IncludeLowerTier")]
            public bool IncludeLowerTier { get; set; }
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

        public bool IncludeLowerTier { get; set; } = false;

        private SolverData solver;

        public SolverDataViewModel(SolverData solver)
        {
            this.solver = solver;
            LoadSettings();
        }

        private const string SettingsFilename = "solver-data-configuration.json";

        private void LoadSettings()
        {
            string filename = Path.Combine(AppContext.BaseDirectory, SettingsFilename);

            if (File.Exists(filename) == false)
                return;

            string jsonContent = File.ReadAllText(filename);

            Configuration conf;

            try
            {
                conf = JsonConvert.DeserializeObject<Configuration>(jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return;
            }

            IncludeLowerTier = conf.IncludeLowerTier;
        }

        private void SaveSettingsToSolver()
        {
            solver.IncludeLowerTier = IncludeLowerTier;
        }

        public void OnClose()
        {
            string jsonContent = JsonConvert.SerializeObject(new Configuration
            {
                IncludeLowerTier = IncludeLowerTier
            }, Formatting.Indented);

            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, SettingsFilename), jsonContent);
            SaveSettingsToSolver();
        }

        public static void LoadSettings(SolverData solver)
        {
            var model = new SolverDataViewModel(solver);
            model.LoadSettings();
            model.SaveSettingsToSolver();
        }

    }
}
