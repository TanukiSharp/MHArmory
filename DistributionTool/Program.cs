using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DistributionTool
{
    class Program
    {
        private const string OutputDistributionsFolder = "Distributions";
        private const string TemporaryDistributionFolderSuffix = "Distribution";
        private const string SolutionFilename = "MHArmory.sln";
        private const string BinaryPath = "MHArmory\\bin";
        private const string BinaryFilename = "MHArmory.exe";
        private const string DistributionRootFolderName = "MHArmory";
        private const string DataFolderName = "data";
        private const string ManifestFilename = "manifest.json";

        static int Main(string[] args)
        {
            return new Program().Run(args);
        }

        private const int RequiredArgumentCount = 1;

        private string buildConfiguration;

        public enum ErrorCodes
        {
            Success = 0,
            InvalidArgumentCount = -1,
            CannotFindSolutionPath = -2,
            CannotFindAssembly = -3,
            CannotLoadAssembly = -4,
            FileCopyFailed = -5,
            OutputDirectoryDeletionFailed = -6,
            OutputArchiveDeletionFailed = -7,
            ZipCreationFailed = -8,
            UpdateManifestFailed = -9,
        }

        private int Run(string[] args)
        {
            if (args.Length < RequiredArgumentCount)
            {
                Console.WriteLine($"Invalid number of arguments, required {RequiredArgumentCount}");
                Console.WriteLine();
                PrintUsage();
                return (int)ErrorCodes.InvalidArgumentCount;
            }

            buildConfiguration = args[0];

            string solutionFullPath = FindSolutionPath(SolutionFilename);

            if (solutionFullPath == null)
            {
                Console.WriteLine($"Could not find solution path based on '{SolutionFilename}' file name.");
                return (int)ErrorCodes.CannotFindSolutionPath;
            }

            string sourceBinaryFullPath = PathCombine(solutionFullPath, BinaryPath, buildConfiguration);

            string assemblyFullFilename = PathCombine(sourceBinaryFullPath, BinaryFilename);

            if (File.Exists(assemblyFullFilename) == false)
            {
                Console.WriteLine($"File '{assemblyFullFilename}' could not be found.");
                Console.WriteLine($"Make sure the project is built, and that build configuration provided '{buildConfiguration}' is correct.");
                Console.WriteLine();
                PrintUsage();
                return (int)ErrorCodes.CannotFindAssembly;
            }

            Assembly assembly = TryLoadAssembly(assemblyFullFilename);
            if (assembly == null)
                return (int)ErrorCodes.CannotLoadAssembly;

            Version assemblyVersion = assembly.GetName().Version;

            Console.WriteLine($"Found assembly version: {assemblyVersion}.");

            string teporaryOutputFolderFullPath = PathCombine(
                solutionFullPath,
                BinaryPath,
                $"{buildConfiguration}{TemporaryDistributionFolderSuffix}"
            );

            string distributionRootFullPath = PathCombine(teporaryOutputFolderFullPath, DistributionRootFolderName);

            string distributionTargetFullPath = PathCombine(distributionRootFullPath, assemblyVersion.ToString());

            if (Directory.Exists(distributionRootFullPath))
            {
                try
                {
                    // Ensures target folder is deleted to start fresh.
                    Directory.Delete(distributionRootFullPath, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not delete existing directory '{distributionRootFullPath}'");
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                    return (int)ErrorCodes.OutputDirectoryDeletionFailed;
                }
            }

            Directory.CreateDirectory(distributionTargetFullPath);

            if (CopyFiles(sourceBinaryFullPath, "dll", distributionTargetFullPath) == false)
                return (int)ErrorCodes.FileCopyFailed;

            // Infer debug mode from the build configuration.
            if (buildConfiguration.Contains("debug", StringComparison.OrdinalIgnoreCase))
            {
                if (CopyFiles(sourceBinaryFullPath, "pdb", distributionTargetFullPath) == false)
                    return (int)ErrorCodes.FileCopyFailed;
            }

            if (CopyFile(assemblyFullFilename, PathCombine(distributionTargetFullPath, BinaryFilename)) == false)
                return (int)ErrorCodes.FileCopyFailed;

            string dataOutputFilePath = Path.Combine(distributionTargetFullPath, DataFolderName);
            if (Directory.Exists(dataOutputFilePath) == false)
                Directory.CreateDirectory(dataOutputFilePath);
            if (CopyFiles(Path.Combine(sourceBinaryFullPath, DataFolderName), "json", dataOutputFilePath) == false)
                return (int)ErrorCodes.FileCopyFailed;

            string targetZipArchiveFilename = $"{DistributionRootFolderName}_{assemblyVersion}.zip";
            string targetZipArchiveFullFilename = PathCombine(solutionFullPath, OutputDistributionsFolder, targetZipArchiveFilename);

            if (File.Exists(targetZipArchiveFullFilename))
            {
                try
                {
                    File.Delete(targetZipArchiveFullFilename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not delete existing zip archive '{targetZipArchiveFullFilename}'");
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                    return (int)ErrorCodes.OutputArchiveDeletionFailed;
                }
            }

            Console.WriteLine($"Creating zip archive '{targetZipArchiveFullFilename}'.");

            try
            {
                ZipFile.CreateFromDirectory(
                    distributionRootFullPath,
                    targetZipArchiveFullFilename,
                    CompressionLevel.Fastest,
                    true
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create output zip archive '{targetZipArchiveFullFilename}'");
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
                return (int)ErrorCodes.ZipCreationFailed;
            }

            bool updateManifestResult = UpdateManifest(
                PathCombine(solutionFullPath, OutputDistributionsFolder, ManifestFilename),
                targetZipArchiveFilename,
                assemblyVersion
            );

            if (updateManifestResult == false)
                return (int)ErrorCodes.UpdateManifestFailed;

            Console.WriteLine("Done.");

            return (int)ErrorCodes.Success;
        }

        private void PrintUsage()
        {
            AssemblyName assemblyName = Assembly.GetEntryAssembly().GetName();
            Console.WriteLine($"Usage: dotnet {assemblyName.Name} <BuildConfiguration>");
            Console.WriteLine();
            Console.WriteLine("<BuildConfiguration>: One of the build configuration setup in the solution of MHArmory.");
            Console.WriteLine();
        }

        private bool CopyFiles(string sourceFullPath, string extension, string targetFullPath)
        {
            if (extension.StartsWith(".") == false)
                extension = $".{extension}";

            string pattern = $"*{extension}";

            foreach (string file in Directory.GetFiles(sourceFullPath, pattern).Where(x => x.EndsWith(extension)))
            {
                if (CopyFile(file, PathCombine(targetFullPath, Path.GetFileName(file))) == false)
                    return false;
            }

            return true;
        }

        private bool CopyFile(string sourceFullFilename, string targetFullFilename)
        {
            Console.WriteLine($"Copying '{targetFullFilename}'.");

            try
            {
                File.Copy(sourceFullFilename, targetFullFilename, true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Copy failed.");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        private Assembly TryLoadAssembly(string assemblyFullFilename)
        {
            Assembly assembly = null;
            Exception assemblyLoadException = null;

            try
            {
                assembly = Assembly.Load(File.ReadAllBytes(assemblyFullFilename));
            }
            catch (Exception ex)
            {
                assemblyLoadException = ex;
            }

            if (assembly == null)
            {
                Console.WriteLine($"Could not load assembly '{assemblyFullFilename}'.");

                if (assemblyLoadException != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(assemblyLoadException.ToString());
                }
            }

            return assembly;
        }

        private string FindSolutionPath(string solutionFilename)
        {
            string currentPath = AppContext.BaseDirectory.TrimEnd('/', '\\');

            while (string.IsNullOrEmpty(currentPath) == false)
            {
                if (File.Exists(PathCombine(currentPath, solutionFilename)))
                    return currentPath;

                currentPath = Path.GetDirectoryName(currentPath);
            }

            return null;
        }

        private string PathCombine(params string[] paths)
        {
            // Make all paths Linux style, since Linux does not
            // support backslashes but Windows supports both.
            return Path.Combine(paths).Replace('\\', '/');
        }

        private bool UpdateManifest(string manifestFullFilename, string targetZipArchiveFilename, Version assemblyVersion)
        {
            try
            {
                SerializeJson(manifestFullFilename, new
                {
                    latestVersion = assemblyVersion.ToString(),
                    latestArchive = targetZipArchiveFilename,
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update manifest '{manifestFullFilename}'");
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

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

                    sw.WriteLine();

                    File.WriteAllText(filename, sw.ToString());
                }
            }
        }
    }
}
