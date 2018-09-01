using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MHArmory.AutoUpdate
{
    public class AutoUpdater
    {
        private static readonly AutoUpdater instance = new AutoUpdater();
        private ILogger logger;

        private const string DownloadBaseUrl = "https://github.com/TanukiSharp/MHArmory/raw/master/Distributions/";
        private const string InfoFilename = "info.json";
        private const string ApplicationDirectoryName = "MHArmory";

        public static void Run(ILogger logger)
        {
            instance.RunInternal(logger);
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
        }

        private async Task<string> DownloadManifestContent()
        {
            var httpClient = new HttpClient();

            HttpResponseMessage response;

            try
            {
                response = await httpClient.GetAsync(DownloadBaseUrl + InfoFilename, HttpCompletionOption.ResponseContentRead);
            }
            catch (HttpRequestException hrex)
            {
                logger?.LogError(hrex, "A network error occurred while downloading the manifest.");
                return null;
            }

            if (response.IsSuccessStatusCode == false)
            {
                logger?.LogError($"A HTTP error occurred while downloading the manifest: {(int)response.StatusCode} ({response.StatusCode}) [{response.ReasonPhrase}].");
                return null;
            }

            try
            {
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Impossible to download manifest content.");
                return null;
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

            if (Path.GetFileName(path) != ApplicationDirectoryName)
                return false;

            return true;
        }

        private bool IsAlternativeDirectoryStructure()
        {
            string path = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return Path.GetFileName(path) == $"{ApplicationDirectoryName}_{App.Version}";
        }
    }
}
