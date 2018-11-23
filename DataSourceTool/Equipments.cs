using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DataSourceTool
{
    public class Equipments
    {
        private ILogger logger;

        private string packageFullPath;

        public async Task Run(string[] args)
        {
            logger = new ConsoleLogger();

            packageFullPath = await GetPackageFullPath();

            if (packageFullPath == null)
            {
                logger?.LogError("Could not determine package path, exiting");
                return;
            }

            Meh();
        }

        private void Meh()
        {
            const string fileToSearch = "\\common\\text\\steam\\armor_eng.gmd";

            bool foundFile = false;

            using (var reader = new BinaryReader(File.OpenRead(packageFullPath), Encoding.UTF8, false))
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

                        if (name != fileToSearch)
                            continue;

                        foundFile = true;

                        if (entryType != 0)
                        {
                            logger?.LogError($"Found file in chunk but unexpected entry type (expected 0, actual is {entryType})");
                            return;
                        }

                        reader.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
                        using (var subReader = new BinaryReader(new SubStream(reader.BaseStream, fileOffset, fileSize), Encoding.UTF8, true))
                            ProcessFile(subReader);

                        break;
                    }

                    if (foundFile)
                        break;
                }
            }

            if (foundFile == false)
                logger?.LogError($"Could not find file '{fileToSearch}'");
        }

        private void ProcessFile(BinaryReader reader)
        {

        }

        private async Task<string> GetPackageFullPath()
        {
            string packageFullPathStoreFile = Path.Combine(AppContext.BaseDirectory, "package_path.txt");

            if (File.Exists(packageFullPathStoreFile))
            {
                string[] lines = await File.ReadAllLinesAsync(packageFullPathStoreFile);
                if (lines != null)
                {
                    string result = lines.FirstOrDefault(x => string.IsNullOrWhiteSpace(x) == false)?.Trim();
                    if (result != null)
                    {
                        if (Directory.Exists(packageFullPathStoreFile) || File.Exists(packageFullPathStoreFile))
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
                Console.WriteLine("Please enter path to chunk0.pkg file: (Ctrl+C to quit)");
                string path = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(path))
                    continue;

                if (File.Exists(path) == false)
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
