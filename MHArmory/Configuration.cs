using System;
using System.IO;
using Newtonsoft.Json;

namespace MHArmory
{
    public class Configuration
    {
        [JsonProperty("selectedAbilities")]
        public int[] SelectedAbilities { get; set; }

        #region Read/Write

        public void Save()
        {
            try
            {
                string filename = Path.Combine(AppContext.BaseDirectory, "config.json");
                string content = JsonConvert.SerializeObject(this);
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
