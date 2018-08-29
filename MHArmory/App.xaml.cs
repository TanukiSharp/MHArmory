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

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Version Version { get; private set; }
        public static string GitBranch { get; private set; }
        public static string GitCommitHash { get; private set; }
        public static string GitRepository { get; private set; }

        static App()
        {
            AssemblyName assemblyName = Assembly.GetEntryAssembly().GetName();

            Version = assemblyName.Version;

            GitBranch = GitInfo.Branch.Trim();
            GitCommitHash = GitInfo.CommitHash.Trim();
            GitRepository = GitInfo.Repository.Trim();
        }

        public static void GetAssemblyInfo(StringBuilder sb)
        {
            AssemblyName assemblyName = Assembly.GetEntryAssembly().GetName();

            sb.AppendFormat("Version: {0}\n", Version);
            sb.AppendFormat("CurrentCulture: {0}\n", Thread.CurrentThread.CurrentCulture);
            sb.AppendFormat("CurrentUICulture: {0}\n", Thread.CurrentThread.CurrentUICulture);
            sb.Append("\n");
            sb.AppendFormat("GitBranch: {0}\n", GitBranch);
            sb.AppendFormat("GitCommitHash: {0}\n", GitCommitHash);
            sb.AppendFormat("GitRepository: {0}\n", GitRepository);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject == null)
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
