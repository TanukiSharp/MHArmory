using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static DataSourceTool.TextMasterDataReader;

namespace DataSourceTool
{
    public class Equipments
    {
        private ILogger logger;

        private string packagesFullPath;
        private readonly HashSet<KeyValueInfo> equipmentNameEntries = new HashSet<KeyValueInfo>();
        private readonly HashSet<ArmorMasterDataEntry> equipmentEntries = new HashSet<ArmorMasterDataEntry>();

        //public void Test()
        //{
        //    using (var reader = new BinaryReader(File.OpenRead(@"<path>")))
        //    {
        //        var masterDataReader = new TextMasterDataReader(reader);
        //        masterDataReader.Read();
        //        int i = 0;
        //        IOrderedEnumerable<EquipmentInfo> items = masterDataReader.Equipments
        //            .OrderBy(x => x.EquipmentType == MHArmory.Core.DataStructures.EquipmentType.Charm)
        //            .ThenBy(x => x.Id)
        //            .ThenBy(x => x.EquipmentType);
        //        foreach (EquipmentInfo x in items)
        //            Console.WriteLine($"[{i++}] {x.Id} {x.Name} ({x.EquipmentType})");
        //    }
        //}

        public async Task Run(string[] args)
        {
            //Test();
            //return;

            logger = new ConsoleLogger();

            packagesFullPath = await GetPackagesFullPath();

            if (packagesFullPath == null)
            {
                logger?.LogError("Could not determine packages path, exiting");
                return;
            }

            string solutionPath = Common.FindSolutionPath();
            if (solutionPath == null)
                throw new InvalidOperationException($"Could not find solution path for '{Common.SolutionFilename}'");

            string outputPath = Path.Combine(solutionPath, "MHArmory", "data");

            if (Directory.Exists(outputPath) == false)
                Directory.CreateDirectory(outputPath);

            IEnumerable<string> packageFilenames = Directory.GetFiles(packagesFullPath, "chunk*.pkg", SearchOption.TopDirectoryOnly)
                .Select(x => new { OriginalFilename = x, Index = GetChunkFileIndex(x) })
                .OrderByDescending(x => x.Index)
                .Select(x => x.OriginalFilename);

            foreach (string packageFilename in packageFilenames)
                ProcessPackage(packageFilename, "\\common\\text\\steam\\armor_eng.gmd", OnTextSubFile);

            if (equipmentNameEntries == null)
            {
                logger?.LogError("Failed to retrieve equipments name entries");
                return;
            }

            foreach (string packageFilename in packageFilenames)
                ProcessPackage(packageFilename, "\\common\\equip\\armor.am_dat", OnEquipmentSubFile);

            var items = equipmentEntries
                .Where(x => x.ArmorType != ArmorType.Layered)
                .OrderBy(x => x.EquipmentType == EquipmentType.Charm)
                .ThenBy(x => x.Index)
                .ThenBy(x => x.EquipmentType)
                .Select(x => new { id = x.Index, type = (int)x.EquipmentType, name = x.Name });

            Common.SerializeJson(Path.Combine(outputPath, "gameEquipments.json"), items);
        }

        private static int GetChunkFileIndex(string filename)
        {
            filename = Path.GetFileNameWithoutExtension(filename);
            return int.Parse(filename.AsSpan(5)); // 5 == 'chunk'.Length
        }

        private void OnTextSubFile(BinaryReader reader)
        {
            var masterDataReader = new TextMasterDataReader(reader);

            foreach (KeyValueInfo x in masterDataReader.Read())
                equipmentNameEntries.Add(x);
        }

        private void OnEquipmentSubFile(BinaryReader reader)
        {
            var masterDataReader = new ArmorMasterDataReader(reader);

            foreach (ArmorMasterDataEntry x in masterDataReader.Read(equipmentNameEntries))
            {
                if (x.Name != null)
                    equipmentEntries.Add(x);
            }
        }

        private void ProcessPackage(string filename, string subFileToSearch, Action<BinaryReader> onFile)
        {
            bool foundFile = false;

            using (var reader = new BinaryReader(File.OpenRead(filename), Encoding.UTF8, false))
            {
                reader.BaseStream.Seek(0x0C, SeekOrigin.Begin);
                int totalParentCount = reader.ReadInt32();
                int totalChildrenCount = reader.ReadInt32();
                reader.BaseStream.Seek(0x100, SeekOrigin.Begin);

                byte[] nameBuffer = new byte[160];

                for (int i = 0; i < totalParentCount; i++)
                {
                    reader.BaseStream.Seek(0x3C, SeekOrigin.Current);

                    long fileSize = reader.ReadInt64();
                    long fileOffset = reader.ReadInt64();
                    int entryType = reader.ReadInt32();
                    int childrenCount = reader.ReadInt32();

                    for (int j = 0; j < childrenCount; j++)
                    {
                        reader.Read(nameBuffer, 0, nameBuffer.Length);
                        fileSize = reader.ReadInt64();
                        fileOffset = reader.ReadInt64();
                        entryType = reader.ReadInt32();
                        reader.BaseStream.Seek(4, SeekOrigin.Current);

                        int trailingZeroIndex = Array.IndexOf<byte>(nameBuffer, 0);
                        string name = Encoding.UTF8.GetString(nameBuffer, 0, trailingZeroIndex);

                        if (name != subFileToSearch)
                            continue;

                        foundFile = true;

                        if (entryType != 0)
                        {
                            logger?.LogError($"Found file in chunk but unexpected entry type (expected 0, actual is {entryType})");
                            return;
                        }

                        reader.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
                        using (var subReader = new BinaryReader(new SubStream(reader.BaseStream, fileOffset, fileSize), Encoding.UTF8, true))
                            onFile(subReader);

                        break;
                    }

                    if (foundFile)
                        break;
                }
            }

            if (foundFile == false)
                logger?.LogWarning($"Could not find file '{subFileToSearch}' in file '{filename}'");
        }

        private async Task<string> GetPackagesFullPath()
        {
            string packageFullPathStoreFile = Path.Combine(AppContext.BaseDirectory, "packages_path.txt");

            if (File.Exists(packageFullPathStoreFile))
            {
                string[] lines = await File.ReadAllLinesAsync(packageFullPathStoreFile);
                if (lines != null)
                {
                    string result = lines.FirstOrDefault(x => string.IsNullOrWhiteSpace(x) == false)?.Trim();
                    if (result != null)
                    {
                        if (Directory.Exists(result))
                            return result;
                        else
                            logger?.LogWarning($"'{packageFullPathStoreFile}' seems to not exists");
                    }
                    else
                        logger?.LogWarning($"File '{packageFullPathStoreFile}' contains invalid data");
                }
                else
                    logger?.LogWarning($"ReadAllLines of file '{packageFullPathStoreFile}' returned null");
            }

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Please enter path to chunk*.pkg files: (Ctrl+C to quit)");
                string path = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(path))
                    continue;

                if (Directory.Exists(path) == false)
                {
                    Console.WriteLine("Not found, please retry");
                    continue;
                }

                File.WriteAllText(packageFullPathStoreFile, path);

                return path;
            }
        }
    }
}
