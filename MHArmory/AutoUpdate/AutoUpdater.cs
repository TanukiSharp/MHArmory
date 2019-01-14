using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MHArmory.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MHArmory.AutoUpdate
{
    public class AutoUpdater
    {
        public static readonly AutoUpdater Instance = new AutoUpdater();

        private ILogger logger;

        private readonly Dispatcher dispatcher;
        private readonly HttpClient httpClient = new HttpClient();

        private const string DownloadBaseUrl = "https://github.com/TanukiSharp/MHArmory/raw/master/Distributions/";
        private const string InfoFilename = "manifest.json";

        public event EventHandler<NewVersionEventArgs> NewVersion;

        public AutoUpdater()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void SetLogger(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this.logger = new DispatcherLogger(dispatcher, logger);
        }

        public void ClearLogger()
        {
            logger = null;
        }

        public void Run()
        {
            Task.Run(() =>
            {
                try
                {
                    RunInternal();
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, ex.Message);
                }
            });
        }

        private async void RunInternal()
        {
            string manifestContent = await DownloadManifestContent();

            if (manifestContent == null)
                return;

            JObject manifest = ParseManifestContent(manifestContent);

            if (manifest == null)
                return;

            if (DetermineVersionAndArchiveName(manifest, out Version version, out string archiveName) == false)
                return;

            if (version > App.Version)
            {
                await dispatcher.BeginInvoke((Action)delegate
                {
                    var downloadUrl = new Uri(DownloadBaseUrl + archiveName, UriKind.Absolute);
                    NewVersion?.Invoke(this, new NewVersionEventArgs(version, downloadUrl));
                });
            }
        }

        private async Task<string> DownloadManifestContent()
        {
            HttpResponseMessage response;

            string url = DownloadBaseUrl + InfoFilename;

            try
            {
                response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"A network error occurred while downloading the manifest at '{url}'.");
                return null;
            }

            if (response.IsSuccessStatusCode == false)
            {
                logger?.LogError($"A HTTP error occurred while downloading the manifest at '{url}': {(int)response.StatusCode} ({response.StatusCode}) [{response.ReasonPhrase}].");
                return null;
            }

            try
            {
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Impossible to download manifest content '{url}'.");
                return null;
            }
        }

        public async Task DownloadAndOpen(Uri url)
        {
            try
            {
                string temporaryFile = $"{Path.GetTempFileName()}.zip";

                if (await DownloadFile(url, temporaryFile) == false)
                    return;

                Process.Start(temporaryFile);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, ex.Message);
            }
        }

        private async Task<bool> DownloadFile(Uri url, string targetFilename)
        {
            HttpResponseMessage response;

            try
            {
                response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead);
            }
            catch (HttpRequestException hrex)
            {
                logger?.LogError(hrex, $"A network error occurred while downloading resource '{url}'.");
                return false;
            }

            if (response.IsSuccessStatusCode == false)
            {
                logger?.LogError($"A HTTP error occurred while downloading resource '{url}': {(int)response.StatusCode} ({response.StatusCode}) [{response.ReasonPhrase}].");
                return false;
            }

            try
            {
                using (var targetStream = new FileStream(targetFilename, FileMode.Create, FileAccess.Write))
                {
                    using (Stream sourceStream = await response.Content.ReadAsStreamAsync())
                    {
                        await sourceStream.CopyToAsync(targetStream);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Failed to download manifest content '{url}'.");
                return false;
            }
        }

        private JObject ParseManifestContent(string manifestContent)
        {
            try
            {
                return JsonConvert.DeserializeObject(manifestContent) as JObject;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to deseriaize manifest JSON content.");
                return null;
            }
        }

        private const string LatestVersionPropertyName = "latestVersion";
        private const string LatestArchivePropertyName = "latestArchive";

        private bool DetermineVersionAndArchiveName(JObject manifest, out Version version, out string archiveName)
        {
            version = new Version();
            archiveName = null;

            var versionValue = manifest[LatestVersionPropertyName] as JValue;
            if (versionValue == null)
            {
                logger?.LogError($"Could not find '{LatestVersionPropertyName}' property in manifest.");
                return false;
            }
            else
            {
                string versionStr = versionValue.Value as string;
                if (versionStr == null)
                {
                    logger?.LogError($"Property '{LatestVersionPropertyName}' is of incorrect type '{(versionValue.Value?.GetType().Name ?? "(null)")}'.");
                    return false;
                }
                else
                {
                    if (Version.TryParse(versionStr, out Version v) == false)
                    {
                        logger?.LogError($"Property '{LatestVersionPropertyName}' is in incorrect format '{versionStr}'.");
                        return false;
                    }

                    version = v;
                }
            }

            var archiveValue = manifest[LatestArchivePropertyName] as JValue;
            if (archiveValue == null)
            {
                logger?.LogError($"Could not find '{LatestArchivePropertyName}' property in manifest.");
                return false;
            }
            else
            {
                string archiveStr = archiveValue.Value as string;
                if (string.IsNullOrWhiteSpace(archiveStr))
                {
                    logger?.LogError($"Property '{LatestArchivePropertyName}' is of incorrect type or value '{(archiveValue.Value?.GetType().Name ?? "(null)")}'.");
                    return false;
                }

                archiveName = archiveStr;
            }

            return true;
        }
    }
}
