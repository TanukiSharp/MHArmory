using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DataSourceTool
{
    public static class Common
    {
        public const string SolutionFilename = "MHArmory.sln";

        public static string FindSolutionPath()
        {
            string currentPath = AppContext.BaseDirectory.TrimEnd('/', '\\');

            while (string.IsNullOrEmpty(currentPath) == false)
            {
                if (File.Exists(Path.Combine(currentPath, SolutionFilename)))
                    return currentPath;

                currentPath = Path.GetDirectoryName(currentPath);
            }

            return null;
        }

        public static void SerializeJson(string filename, object instance)
        {
            using (var sw = new StringWriter())
            {
                using (var jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Formatting.Indented;
                    jw.IndentChar = ' ';
                    jw.Indentation = 4;

                    var serializer = new JsonSerializer
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    serializer.Serialize(jw, instance);

                    File.WriteAllText(filename, sw.ToString());
                }
            }
        }
    }
}
