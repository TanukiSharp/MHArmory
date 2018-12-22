using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using MHArmory.ArmoryDataSource.DataStructures;
using Newtonsoft.Json;

namespace DataSourceTool
{
    class Program
    {
        static void Main(string[] args)
        {
            //// Loads data from Athena ASS and export to Armory format
            new Exporter().Run(args).Wait();

            //// Export the gameEquipments.json file from chunk*.pkg files
            //new Equipments().Run(args).Wait();

            //// Export the skills.json file
            //new ExporterSkills().Run(args).Wait();

            //// Compare Athena ASS et Armory data sources
            //new Comparer().Run(args).Wait();
        }
    }
}
