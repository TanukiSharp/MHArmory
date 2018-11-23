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

        public async Task Run(string[] args)
        {
            logger = new ConsoleLogger();

            string gameFullPath = await GetGameFullPath();

            if (gameFullPath == null)
            {
                logger?.LogError("Could not determine game path, exiting");
                return;
            }

            OodleLZ_Decompress decompressFunc = GetDecompressionFunction(gameFullPath);
            if (decompressFunc == null)
                return;
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        private extern static IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private extern static IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool FreeLibrary(IntPtr hModule);

        private delegate int OodleLZ_Decompress(byte[] buffer, long bufferSize, byte[] outputBuffer, long outputBufferSize, uint a, uint b, ulong c, uint d, uint e, uint f, uint g, uint h, uint i, uint threadModule);

        private OodleLZ_Decompress GetDecompressionFunction(string libraryPath)
        {
            IntPtr library = LoadLibrary(Path.Combine(libraryPath, "oo2core_5_win64.dll"));

            if (library == IntPtr.Zero)
            {
                logger?.LogError($"Failed to load library 'oo2core_5_win64.dll'");
                return null;
            }

            IntPtr procAddress = GetProcAddress(library, "OodleLZ_Decompress");

            if (procAddress == IntPtr.Zero)
            {
                logger?.LogError($"Failed to load function 'OodleLZ_Decompress'");
                return null;
            }

            return Marshal.GetDelegateForFunctionPointer<OodleLZ_Decompress>(procAddress);
        }

        private async Task<string> GetGameFullPath()
        {
            string gameFullPathStoreFile = Path.Combine(AppContext.BaseDirectory, "gamepath.txt");

            if (File.Exists(gameFullPathStoreFile))
            {
                string[] lines = await File.ReadAllLinesAsync(gameFullPathStoreFile);
                if (lines != null)
                {
                    string result = lines.FirstOrDefault(x => string.IsNullOrWhiteSpace(x) == false)?.Trim();
                    if (result != null)
                    {
                        if (Directory.Exists(gameFullPathStoreFile) || File.Exists(gameFullPathStoreFile))
                            return result;
                        else
                            logger?.LogWarning($"'{gameFullPathStoreFile}' seems to not exists");
                    }
                    else
                        logger?.LogWarning($"File '{gameFullPathStoreFile}' contains invalid data");
                }
                else
                    logger?.LogWarning($"ReadAllLines of file '{gameFullPathStoreFile}' returned null");
            }

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Please enter path to MonsterHunterWorld.exe file: (Ctrl+C to quit)");
                string path = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(path))
                    continue;

                if (Directory.Exists(path) == false && File.Exists(path) == false)
                {
                    Console.WriteLine("Not found, please retry");
                    continue;
                }

                path = Common.FindPathContainingFile(path, "MonsterHunterWorld.exe");

                if (path == null)
                {
                    Console.WriteLine("Invalid path, please retry");
                    continue;
                }

                File.WriteAllText(gameFullPathStoreFile, path);

                return path;
            }
        }
    }
}
