using System;
using System.Collections.Generic;
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
        private static readonly AutoUpdater instance = new AutoUpdater();
        private static Dispatcher dispatcher;

        private ILogger logger;
        private readonly HttpClient httpClient = new HttpClient();

        private const string DownloadBaseUrl = "https://github.com/TanukiSharp/MHArmory/raw/master/Distributions/";
        private const string InfoFilename = "manifest.json";

        public static void Run(ILogger logger)
        {
            dispatcher = Dispatcher.CurrentDispatcher;

            logger = new DispatcherLogger(dispatcher, logger);
            Task.Run(() => instance.RunInternal(logger));
        }

        private async void RunInternal(ILogger logger)
        {
            this.logger = logger;

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
                // TODO: download in the background, extract and do something

                await dispatcher.BeginInvoke((Action)delegate
                {
                    MessageBox.Show(
                        $"You are currently using the version {App.Version}\n" +
                        $"A newer version is availabe: {version}\n" +
                        "\n" +
                        "Note: The auto-updater is not finished yet so for now you are just informed.",
                        "A newer version is available",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
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
            catch (HttpRequestException hrex)
            {
                logger?.LogError(hrex, $"A network error occurred while downloading the manifest at '{url}'.");
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

        private async Task<bool> DownloadFile(string url, string targetFilename)
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
                var versionStr = versionValue.Value as string;
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
                var archiveStr = archiveValue.Value as string;
                if (string.IsNullOrWhiteSpace(archiveStr))
                {
                    logger?.LogError($"Property '{LatestArchivePropertyName}' is of incorrect type or value '{(archiveValue.Value?.GetType().Name ?? "(null)")}'.");
                    return false;
                }

                archiveName = archiveStr;
            }

            return true;
        }

        private void AnalyzeDirectoryStructure()
        {
            if (IsDefaultDirectoryStructure())
            {
            }
        }

        private bool IsDefaultDirectoryStructure()
        {
            string path = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (Path.GetFileName(path) != App.Version.ToString())
                return false;

            path = Path.GetDirectoryName(path);

            if (Path.GetFileName(path) != App.ApplicationName)
                return false;

            return true;
        }

        private bool IsAlternativeDirectoryStructure()
        {
            string path = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return Path.GetFileName(path) == $"{App.ApplicationName}_{App.Version}";
        }
    }
}
