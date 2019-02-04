using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MHArmory
{
    public static class Aliases
    {
        private static readonly IDictionary<string, string> EmptyDictionary = new Dictionary<string, string>();

        public static IDictionary<string, string> Load()
        {
            string aliasesFullFilename = Path.Combine(AppContext.BaseDirectory, "aliases.json");

            if (File.Exists(aliasesFullFilename) == false)
                return EmptyDictionary;

            string content = File.ReadAllText(aliasesFullFilename);

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid JSON content: {ex.Message}");
                return EmptyDictionary;
            }
        }
    }
}
