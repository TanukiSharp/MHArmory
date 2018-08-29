using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DistributionTool
{
    class Program
    {
        private const string DistributionFolderSuffix = "Distribution";
        private const string SolutionFilename = "MHArmory.sln";
        private const string BinaryPath = "MHArmory\\bin";
        private const string BinaryFilename = "MHArmory.exe";
        private const string DistributionRootFolderName = "MHArmory";

        static int Main(string[] args)
        {
            args = new string[] { "Debug" };

            return new Program().Run(args);
        }

        private const int RequiredArgumentCount = 1;

        private string buildConfiguration;

        private int Run(string[] args)
        {
            if (args.Length < RequiredArgumentCount)
            {
                Console.WriteLine($"Invalid number of arguments, required {RequiredArgumentCount}");
                Console.WriteLine();
                PrintUsage();
                return -1;
            }

            buildConfiguration = args[0];

            string solutionFullPath = FindSolutionPath(SolutionFilename);

            if (solutionFullPath == null)
            {
                Console.WriteLine($"Could not find solution path based on '{SolutionFilename}' file name.");
                return -2;
            }

            string sourceBinaryFullPath = Path.Combine(solutionFullPath, BinaryPath, buildConfiguration);

            string assemblyFullFilename = Path.Combine(sourceBinaryFullPath, BinaryFilename);

            if (File.Exists(assemblyFullFilename) == false)
            {
                Console.WriteLine($"File '{assemblyFullFilename}' could not be found.");
                Console.WriteLine($"Make sure the project is build, and that build configuration provided '{buildConfiguration}' is correct.");
                Console.WriteLine();
                PrintUsage();
                return -3;
            }

            Assembly assembly = TryLoadAssembly(assemblyFullFilename);
            if (assembly == null)
                return -4;

            Version assemblyVersion = assembly.GetName().Version;

            Console.WriteLine($"Found assembly version: {assemblyVersion}.");

            string distributionTargetFullPath = Path.Combine(
                solutionFullPath,
                BinaryPath,
                $"{buildConfiguration}{DistributionFolderSuffix}",
                DistributionRootFolderName,
                assemblyVersion.ToString()
            );

            if (Directory.Exists(distributionTargetFullPath))
            {
                // Ensures target folder is deleted to start fresh.
                Directory.Delete(distributionTargetFullPath, true);
            }

            Directory.CreateDirectory(distributionTargetFullPath);

            if (CopyFiles(sourceBinaryFullPath, "dll", distributionTargetFullPath) == false)
                return -5;

            // Infer debug mode from the build configuration.
            if (buildConfiguration.Contains("debug", StringComparison.OrdinalIgnoreCase))
            {
                if (CopyFiles(sourceBinaryFullPath, "pdb", distributionTargetFullPath) == false)
                    return -5;
            }

            if (CopyFile(assemblyFullFilename, Path.Combine(distributionTargetFullPath, BinaryFilename)) == false)
                return -5;

            string outputFolderFullPath = Path.Combine(
                solutionFullPath,
                BinaryPath,
                $"{buildConfiguration}{DistributionFolderSuffix}"
            );

            string sourceFolderToZipFullPath = Path.Combine(outputFolderFullPath, DistributionRootFolderName);
            string targetZipArchiveFullFilename = Path.Combine(outputFolderFullPath, $"{DistributionRootFolderName}_{assemblyVersion}.zip");

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
                    return -6;
                }
            }

            ZipFile.CreateFromDirectory(
                sourceFolderToZipFullPath,
                targetZipArchiveFullFilename,
                CompressionLevel.Fastest,
                true
            );

            return 0;
        }

        private void PrintUsage()
        {
        }

        private bool CopyFiles(string sourceFullPath, string extension, string targetFullPath)
        {
            if (extension.StartsWith(".") == false)
                extension = $".{extension}";

            string pattern = $"*{extension}";

            foreach (string file in Directory.GetFiles(sourceFullPath, pattern).Where(x => x.EndsWith(extension)))
            {
                if (CopyFile(file, Path.Combine(targetFullPath, Path.GetFileName(file))) == false)
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
                if (File.Exists(Path.Combine(currentPath, solutionFilename)))
                    return currentPath;

                currentPath = Path.GetDirectoryName(currentPath);
            }

            return null;
        }
    }
}
