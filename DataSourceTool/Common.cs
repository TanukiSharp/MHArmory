using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DataSourceTool
{
    public static class Common
    {
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
