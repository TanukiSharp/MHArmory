using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MHArmory.Search.Contracts;

namespace MHArmory.Search.OpenCL
{
    public class OpenCLSearch : ISolver
    {
        public string Name { get; } = "OpenCL solver";
        public string Author { get; } = "Gediminas Masaitis";
        public string Description { get; } = "GPU accelerated solver";
        public int Version { get; } = 1;

        // No reasonable way of tracking progress from within OpenCL
        #pragma warning disable 0067
        public event Action<double> SearchProgress;
        #pragma warning restore 0067

        private readonly SearchDataSerializer serializer;
        private readonly SearchResultDeserializer deserializer;
        private readonly ISerializedSearch serializedSearch;

        public static OpenCLSearch Instance { get; } = new OpenCLSearch();

        private OpenCLSearch()
        {
            serializer = new SearchDataSerializer();
            deserializer = new SearchResultDeserializer();
            serializedSearch = new Host(new AutomaticDeviceResolver());
        }

        public Task<IList<ArmorSetSearchResult>> SearchArmorSets(ISolverData solverData, CancellationToken cancellationToken)
        {
            SerializedSearchParameters serializedData = serializer.Serialize(solverData);
            //DumpSerializedData(serializedData); 
            SerializedSearchResults serializedResults = serializedSearch.Run(serializedData);
            IList<ArmorSetSearchResult> results = deserializer.Deserialize(solverData, serializedData.SearchIDMaps, serializedResults);
            var resultsTask = Task.FromResult(results);
            return resultsTask;
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
    }
}
