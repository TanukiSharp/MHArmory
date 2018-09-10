using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MHArmory.AutoUpdate;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string ApplicationName { get; private set; }
        public static Version Version { get; private set; }
        public static string DisplayVersion { get; private set; }
        public static string GitBranch { get; private set; }
        public static string GitCommitHash { get; private set; }
        public static string GitRepository { get; private set; }

        public static bool HasWriteAccess { get; private set; }

        static App()
        {
            AssemblyName assemblyName = Assembly.GetEntryAssembly().GetName();

            ApplicationName = assemblyName.Name;
            Version = assemblyName.Version;
            DisplayVersion = CreateDisplayVersion(Version);

            GitBranch = GitInfo.Branch.Trim();
            GitCommitHash = GitInfo.CommitHash.Trim();
            GitRepository = GitInfo.Repository.Trim();

            HasWriteAccess = WriteTest();

            //AutoUpdater.Run(null);
        }

        private static string CreateDisplayVersion(Version version)
        {
            if (version.Revision > 0)
                return version.ToString();

            if (version.Build > 0)
                return $"{version.Major}.{version.Minor}.{version.Build}";

            return $"{version.Major}.{version.Minor}";
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            if (HasWriteAccess == false)
            {
                var sb = new StringBuilder();
                sb.Append("The application requires write access to store your settings.\n");
                sb.Append("\n");
                sb.Append("It can run without, but all preferences will not be persisted and disappear once you close the application.\n");
                sb.Append("\n");
                sb.Append("It is recommended to close the application and move it to a location where it has write access");

                MessageBox.Show(sb.ToString(), "Write access", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static bool WriteTest()
        {
            try
            {
                string file = Path.Combine(AppContext.BaseDirectory, Guid.NewGuid().ToString("N"));
                File.WriteAllText(file, "test");
                File.Delete(file);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void GetAssemblyInfo(StringBuilder sb)
        {
            AssemblyName assemblyName = Assembly.GetEntryAssembly().GetName();

            sb.AppendFormat("Version: {0}\n", Version);
            sb.AppendFormat("CurrentCulture: {0}\n", Thread.CurrentThread.CurrentCulture);
            sb.AppendFormat("CurrentUICulture: {0}\n", Thread.CurrentThread.CurrentUICulture);
            sb.AppendFormat("GitBranch: {0}\n", GitBranch);
            sb.AppendFormat("GitCommitHash: {0}\n", GitCommitHash);
            sb.AppendFormat("GitRepository: {0}\n", GitRepository);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject == null || HasWriteAccess == false)
                return;

            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "error_logs");

                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);

                string filename = Path.Combine(path, $"{DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss_fff")}.log");

                var sb = new StringBuilder();

                sb.AppendFormat("Date: {0}\n", DateTimeOffset.Now.ToString());
                GetAssemblyInfo(sb);
                sb.Append("\n");
                sb.AppendFormat("{0}\n", e.ExceptionObject.ToString());
                if (e.ExceptionObject is Exception ex)
                {
                    sb.Append("\n");
                    sb.Append("-----\n");
                    sb.Append("\n");
                    sb.AppendFormat("{0}\n", ex.Demystify());
                }

                File.WriteAllText(filename, sb.ToString());

                sb.Clear();
                sb.Append("The application encountered an unexpected critical error and will terminate.\n");
                sb.Append("\n");
                sb.Append("A file containing important information to help the author(s) understand and fix the problem has been created.\n");
                sb.Append("\n");
                sb.Append("This file does not contain any private information, but feel free to review its content before sending it to the author(s).\n");
                sb.Append("\n");
                sb.Append($"Location: '{filename}'");

                MessageBox.Show(sb.ToString(), "Unexpected critical error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Helps the process to die much faster (sometimes it hangs for a very long time)
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                File.WriteAllText($"fatal_{DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss_fff")}.log", $"{DateTimeOffset.Now.ToString()}\n{ex.ToString()}\n");
            }
        }
    }
}
