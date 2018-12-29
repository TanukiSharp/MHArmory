using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MHArmory.Search.OpenCL
{
    public class OpenCLSearch
    {
        private readonly SearchDataSerializer serializer;
        private readonly SearchResultDeserializer deserializer;
        private readonly Host host;

        public static OpenCLSearch Instance { get; } = new OpenCLSearch();

        private OpenCLSearch()
        {
            serializer = new SearchDataSerializer();
            deserializer = new SearchResultDeserializer();
            host = new Host(new AutomaticDeviceResolver());
        }

        // For debugging purposes
        private void WriteByteArray(StreamWriter writer, string title, byte[] data)
        {
            string str = string.Join(", ", data.Select(x => $"0x{x:X2}"));
            writer.WriteLine($"auto {title} = std::vector<uint8_t> {{{str}}};");
            writer.WriteLine();
        }

        // For debugging purposes
        private void DumpSerializedData(SerializedSearchParameters searchParameters)
        {
            using (FileStream file = File.OpenWrite("dump.txt"))
            {
                var writer = new StreamWriter(file);
                WriteByteArray(writer, "header", searchParameters.Header);
                WriteByteArray(writer, "equipment", searchParameters.Equipment);
                WriteByteArray(writer, "decorations", searchParameters.Decorations);
                WriteByteArray(writer, "desired_skills", searchParameters.DesiredSkills);
                writer.Flush();
            }
        }

        public List<ArmorSetSearchResult> Run(ISolverData data)
        {
            SerializedSearchParameters serializedData = serializer.Serialize(data);
            //DumpSerializedData(serializedData); 
            SerializedSearchResults serializedResults = host.Run(serializedData);
            List<ArmorSetSearchResult> results = deserializer.Deserialize(data, serializedData.SearchIDMaps, serializedResults);
            return results;
        }
    }
}
