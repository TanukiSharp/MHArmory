using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.Search.OpenCL
{
    public class OpenCLSearch
    {
        private SearchDataSerializer Serializer { get; }
        private SearchResultDeserializer Deserializer { get; }
        private Host Host { get; }

        private static OpenCLSearch _instance;
        public static OpenCLSearch Instance
        {
            get
            {
                return _instance ?? (_instance = new OpenCLSearch());
            }
        }

        private OpenCLSearch()
        {
            Serializer = new SearchDataSerializer();
            Deserializer = new SearchResultDeserializer();
            Host = new Host();
        }

        private void WriteByteArr(StreamWriter writer, string title, byte[] data)
        {
            var str = data.Select(x => $"0x{x:X2}").Aggregate((c, n) => c + ", " + n);
            writer.WriteLine($"auto {title} = std::vector<uint8_t> {{{str}}};");
            writer.WriteLine();
        }

        private void DumpSerializedData(SerializedSearchParameters searchParameters)
        {
            using (var file = File.OpenWrite("dump.txt"))
            {
                var writer = new StreamWriter(file);
                WriteByteArr(writer, "header", searchParameters.Header);
                WriteByteArr(writer, "equipment", searchParameters.Equipment);
                WriteByteArr(writer, "decorations", searchParameters.Decorations);
                WriteByteArr(writer, "desired_skills", searchParameters.DesiredSkills);
                writer.Flush();
            }
        }

        public List<ArmorSetSearchResult> Run(ISolverData data)
        {
            var serializedData = Serializer.Serialize(data);
            //DumpSerializedData(serializedData); // for testing purposes
            var serializedResults = Host.Run(serializedData);
            var results = Deserializer.Deserialize(data, serializedData.SearchIDMaps, serializedResults);
            return results;
        }
    }
}
