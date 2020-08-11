using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using MHArmory.ArmoryDataSource.DataStructures;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DataSourceTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new GameMasterDataImporter().Run();

            //// Loads data from Athena ASS and export to Armory format
            //new Exporter().Run(args).Wait();

            //// Export the gameEquipments.json file from chunk*.pkg files
            //new Equipments().Run(args).Wait();

            //// Export the skills.json file
            //new ExporterSkills().Run(args).Wait();

            //// Compare Athena ASS et Armory data sources
            //new Comparer().Run(args).Wait();
        }
    }
}
