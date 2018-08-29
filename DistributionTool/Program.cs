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
            ZipCreationFailed = -8
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

            string sourceBinaryFullPath = Path.Combine(solutionFullPath, BinaryPath, buildConfiguration);

            string assemblyFullFilename = Path.Combine(sourceBinaryFullPath, BinaryFilename);

            if (File.Exists(assemblyFullFilename) == false)
            {
                Console.WriteLine($"File '{assemblyFullFilename}' could not be found.");
                Console.WriteLine($"Make sure the project is build, and that build configuration provided '{buildConfiguration}' is correct.");
                Console.WriteLine();
                PrintUsage();
                return (int)ErrorCodes.CannotFindAssembly;
            }

            Assembly assembly = TryLoadAssembly(assemblyFullFilename);
            if (assembly == null)
                return (int)ErrorCodes.CannotLoadAssembly;

            Version assemblyVersion = assembly.GetName().Version;

            Console.WriteLine($"Found assembly version: {assemblyVersion}.");

            string outputFolderFullPath = Path.Combine(
                solutionFullPath,
                BinaryPath,
                $"{buildConfiguration}{DistributionFolderSuffix}"
            );

            string distributionRootFullPath = Path.Combine(outputFolderFullPath, DistributionRootFolderName);

            string distributionTargetFullPath = Path.Combine(distributionRootFullPath, assemblyVersion.ToString());

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

            if (CopyFile(assemblyFullFilename, Path.Combine(distributionTargetFullPath, BinaryFilename)) == false)
                return (int)ErrorCodes.FileCopyFailed;

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
