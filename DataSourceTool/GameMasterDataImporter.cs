using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace DataSourceTool
{
    public class MasterDataDownloader
    {
        private readonly HttpClient httpClient;
        private readonly string cachePath;

        public MasterDataDownloader()
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://raw.githubusercontent.com/TanukiSharp/MHWMasterDataUtils/master/MHWMasterDataUtils.Exporter/data/")
            };

            cachePath = Path.Join(AppContext.BaseDirectory, "cache");

            if (Directory.Exists(cachePath) == false)
                Directory.CreateDirectory(cachePath);
        }

        private bool ShouldDownload(string cacheFile)
        {
            if (File.Exists(cacheFile) == false)
                return true;

            if (File.GetLastWriteTimeUtc(cacheFile) < DateTime.UtcNow.Subtract(TimeSpan.FromHours(4.0)))
                return true;

            return false;
        }

        public async Task<string> DownloadFile(string relativeFilename)
        {
            string cacheFile = Path.Join(cachePath, relativeFilename);

            string content;

            if (ShouldDownload(cacheFile))
            {
                content = await httpClient.GetStringAsync(relativeFilename);
                File.WriteAllText(cacheFile, content);
            }
            else
                content = File.ReadAllText(cacheFile);

            return content;
        }
    }

    public class GameMasterDataImporter
    {
        private readonly MasterDataDownloader downloader = new MasterDataDownloader();

        public async Task Run()
        {
            string content = await downloader.DownloadFile("charms.json");

        }
    }
}
