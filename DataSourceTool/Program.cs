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
            //new Exporter().Run(args).Wait();
            new Equipments().Run(args).Wait();
            //new ExporterSkills().Run(args).Wait();
            //new Comparer().Run(args).Wait();
        }
    }
}
