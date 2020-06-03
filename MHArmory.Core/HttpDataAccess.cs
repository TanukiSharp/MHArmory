using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MHArmory.Core
{
    public class HttpDataAccess
    {
        private static HttpClient httpClient;

        private static readonly string baseCachePath = Path.Combine(AppContext.BaseDirectory, "cache");

        private const string TimestampFormat = "yyyyMMddHHmmss";

        private readonly ILogger logger;
        private readonly bool hasWriteAccess;
        private readonly Func<HttpClient, Task<string>> dataProducer;
        private readonly string cachePath;
        private readonly TimeSpan cacheDuration;

        public HttpDataAccess(ILogger logger, bool hasWriteAccess, string dataSourceName, TimeSpan cacheDuration, Func<HttpClient, Task<string>> dataProducer)
        {
            this.logger = logger;
            this.hasWriteAccess = hasWriteAccess;

            if (hasWriteAccess)
            {
                cachePath = Path.Combine(baseCachePath, dataSourceName);

                if (Directory.Exists(cachePath) == false)
                {
                    try
                    {
                        Directory.CreateDirectory(cachePath);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            this.cacheDuration = cacheDuration;
            this.dataProducer = dataProducer;
        }

        public async Task<string> GetRawData(string api)
        {
            string result = ReadRawDataFromCache(api, logger);

            if (result != null)
                return result;

            if (httpClient == null)
            {
                httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(15.0)
                };
            }

            try
            {
                result = await dataProducer(httpClient);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
                return null;
            }

            WriteRawDataToCache(result, api, logger);

            return result;
        }

        public void InvalidateCache(string api)
        {
            if (hasWriteAccess == false)
                return;

            string[] files;

            try
            {
                files = GetInvolvedFiles(api);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
                return;
            }

            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex.ToString());
                }
            }
        }

        private string[] GetInvolvedFiles(string api)
        {
            return Directory.GetFiles(cachePath, $"{api.Replace('/', '_')}.*.json");
        }

        private string ReadRawDataFromCache(string api, ILogger logger)
        {
            if (!hasWriteAccess)
                return null;

            string[] files;

            try
            {
                files = GetInvolvedFiles(api);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
                return null;
            }

            if (files.Length == 0)
            {
                // no cache data
                return null;
            }

            if (files.Length > 1 && hasWriteAccess)
            {
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex.ToString());
                    }
                }

                return null;
            }

            string timestampString = Path.GetFileNameWithoutExtension(files[0]).Substring(api.Length + 1); // remove heading api name and dot

            if (hasWriteAccess)
            {
                if (DateTime.TryParseExact(timestampString, TimestampFormat, null, DateTimeStyles.AdjustToUniversal, out DateTime timestamp) == false ||
                    timestamp.Add(cacheDuration) < DateTime.UtcNow)
                {
                    try
                    {
                        File.Delete(files[0]);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex.ToString());
                    }

                    return null;
                }
            }

            try
            {
                return File.ReadAllText(files[0]);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
                return null;
            }
        }

        private void WriteRawDataToCache(string content, string api, ILogger logger)
        {
            if (hasWriteAccess == false)
                return;

            string filename = Path.Combine(cachePath, $"{api.Replace('/', '_')}.{DateTime.UtcNow.ToString(TimestampFormat)}.json");

            try
            {
                File.WriteAllText(filename, content);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
            }
        }
    }
}
