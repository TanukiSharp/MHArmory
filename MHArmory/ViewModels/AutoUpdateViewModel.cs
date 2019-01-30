using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.AutoUpdate;
using MHArmory.Configurations;
using MHArmory.Core.WPF;
using Microsoft.Extensions.Logging;

namespace MHArmory.ViewModels
{
    public class AutoUpdateViewModel : ViewModelBase
    {
        private bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            private set { SetValue(ref isVisible, value); }
        }

        private string shortText;
        public string ShortText
        {
            get { return shortText; }
            private set { SetValue(ref shortText, value); }
        }

        private string detailedText;
        public string DetailedText
        {
            get { return detailedText; }
            private set { SetValue(ref detailedText, value); }
        }

        public ICommand AcknowledgeCommand { get; }
        public ICommand DownloadCommand { get; }

        private NewVersionEventArgs newVersion;

        public AutoUpdateViewModel()
        {
            AcknowledgeCommand = new AnonymousCommand(OnAcknowledge);
            DownloadCommand = new AnonymousCommand(OnDownload);

            AutoUpdater autoUpdater = AutoUpdater.Instance;

            autoUpdater.SetLogger(new AutoUpdateLogger(this));

            autoUpdater.NewVersion += AutoUpdater_NewVersion;
            autoUpdater.Run();
        }

        private void EnableCommands()
        {
            ((AnonymousCommand)AcknowledgeCommand).IsEnabled = true;
            ((AnonymousCommand)DownloadCommand).IsEnabled = true;
        }

        private void DisableCommands()
        {
            ((AnonymousCommand)AcknowledgeCommand).IsEnabled = false;
            ((AnonymousCommand)DownloadCommand).IsEnabled = false;
        }

        private void AutoUpdater_NewVersion(object sender, NewVersionEventArgs e)
        {
            Version.TryParse(GlobalData.Instance.Configuration?.AcknowledgedVersion, out Version acknowledgedVersion);

            if (acknowledgedVersion == null || acknowledgedVersion < e.NewVersion)
            {
                IsVisible = true;
                ShortText = $"New version available: {e.NewVersion}";
                DetailedText = null;
            }

            newVersion = e;
        }

        private void OnAcknowledge()
        {
            IsVisible = false;

            if (newVersion != null)
            {
                GlobalData.Instance.Configuration.AcknowledgedVersion = newVersion.NewVersion.ToString();
                ConfigurationManager.Save(GlobalData.Instance.Configuration);
            }
        }

        private async void OnDownload()
        {
            if (newVersion != null)
            {
                ShortText = $"Downloading version {newVersion.NewVersion}...";

                DisableCommands();

                try
                {
                    await AutoUpdater.Instance.DownloadAndOpen(newVersion.DownloadUrl);
                    IsVisible = false;
                }
                finally
                {
                    EnableCommands();
                }
            }
        }

        private class AutoUpdateLogger : ILogger
        {
            private readonly AutoUpdateViewModel parent;

            public AutoUpdateLogger(AutoUpdateViewModel parent)
            {
                this.parent = parent;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                parent.IsVisible = true;
                parent.ShortText = "Auto-update error!";

                if (exception == null)
                    parent.DetailedText = formatter(state, null);
                else
                    parent.DetailedText = $"{formatter(state, exception)}\n\n{exception.ToString()}";
            }
        }
    }
}
