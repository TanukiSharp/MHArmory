using System;
using System.IO;
using System.Threading.Tasks;

namespace DistributionTool
{
    class Program
    {
        private const string SolutionFilename = "MHArmory.sln";
        private const string BinaryPath = "MHArmory\\bin";
        private const string BinaryFilename = "MHArmory.exe";

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

            string assemblyFullFilename = Path.Combine(solutionFullPath, BinaryPath, buildConfiguration, BinaryFilename);

            if (File.Exists(assemblyFullFilename) == false)
            {
                Console.WriteLine($"File '{assemblyFullFilename}' could not be found.");
                Console.WriteLine($"Make sure the project is build, and that build configuration provided '{buildConfiguration}' is correct.");
                Console.WriteLine();
                PrintUsage();
                return -3;
            }

            return 0;
        }

        private void PrintUsage()
        {
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
