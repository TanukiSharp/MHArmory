using System;
using System.IO;
using Newtonsoft.Json;

namespace MHArmory
{
    public class InParametersConfiguration
    {
        [JsonProperty("weaponSlots")]
        public int[] WeaponSlots { get; set; }
    }

    public class Configuration
    {
        [JsonProperty("selectedAbilities")]
        public int[] SelectedAbilities { get; set; }

        [JsonProperty("inParameters")]
        public InParametersConfiguration InParameters { get; } = new InParametersConfiguration();

        #region Read/Write

        public void Save()
        {
            try
            {
                string filename = Path.Combine(AppContext.BaseDirectory, "config.json");
                string content = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filename, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static Configuration Load()
        {
            try
            {
                string filename = Path.Combine(AppContext.BaseDirectory, "config.json");
                if (File.Exists(filename))
                {
                    string content = File.ReadAllText(filename);
                    return JsonConvert.DeserializeObject<Configuration>(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return new Configuration();
        }

        #endregion
    }
}
