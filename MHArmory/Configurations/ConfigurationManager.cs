using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MHArmory.Configurations
{
    public static class ConfigurationManager
    {
        private static ILogger logger;

        public static void SetLogger(ILogger logger)
        {
            ConfigurationManager.logger = logger;
        }

        public static bool IsBrandNewConfiguration { get; private set; }

        public static void Save(IConfiguration configurationObject)
        {
            if (App.HasWriteAccess == false)
                return;

            try
            {
                string filename = Path.Combine(AppContext.BaseDirectory, "config.json");
                string content = JsonConvert.SerializeObject(configurationObject, Formatting.Indented);

                try
                {
                    File.WriteAllText(filename, content);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, $"Failed to write configuration file '{filename}'.");
                    return; // do not proceed to backups if write failed
                }

                if (configurationObject.BackupLocations != null)
                {
                    foreach (string x in configurationObject.BackupLocations)
                    {
                        filename = x;

                        if (Path.IsPathRooted(filename) == false)
                            filename = Path.Combine(AppContext.BaseDirectory, filename);

                        string path = Path.GetDirectoryName(filename);
                        if (Directory.Exists(path) == false)
                        {
                            try
                            {
                                Directory.CreateDirectory(path);
                            }
                            catch (Exception ex)
                            {
                                logger?.LogError(ex, $"Failed to create directory for configuration file backup '{path}'.");
                                continue;
                            }
                        }

                        try
                        {
                            File.WriteAllText(filename, content);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError(ex, $"Failed to write configuration file backup '{filename}'.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"An error occured when trying to save configuration file.");
            }
        }

        private const string ConfigurationTypePrefix = "ConfigurationV";

        public static T Load<T>() where T : IConfiguration, new()
        {
            string typeName = typeof(T).Name;

            if (typeName.StartsWith(ConfigurationTypePrefix) == false)
                throw new InvalidOperationException($"Invalid configuration type provided '{typeof(T).FullName}', bad prefix.");

            string versionStr = typeName.Substring(ConfigurationTypePrefix.Length);

            if (uint.TryParse(versionStr, out uint applicationVersion) == false)
                throw new InvalidOperationException($"Invalid configuration type provided '{typeof(T).FullName}', invalid version number '{versionStr}'.");

            string filename = null;
            Exception failFast = null;

            try
            {
                filename = Path.Combine(AppContext.BaseDirectory, "config.json");
                if (File.Exists(filename))
                {
                    string content = File.ReadAllText(filename);

                    int configVersion = ReadFileVersion(content);

                    if (configVersion < 0)
                        configVersion = (int)applicationVersion;

                    if (configVersion == applicationVersion)
                        return JsonConvert.DeserializeObject<T>(content);

                    if (configVersion > applicationVersion)
                        failFast = new InvalidDataException($"Invalid configuration file version '{configVersion}', latest supported version is '{applicationVersion}'.");
                    else
                    {
                        // FIXME: Temporary solution
                        if (configVersion == 2 && applicationVersion == 3)
                        {
                            ConfigurationV2 configV2 = JsonConvert.DeserializeObject<ConfigurationV2>(content);

                            Conversion.IConverter converter = new Conversion.ConverterV2ToV3();
                            var config = converter.Convert(configV2) as IConfiguration;
                            if (config != null)
                                return (T)config;
                        }

                        // TODO Here version is between 1 and 'version - 1', implement auto conversion of configuration formats
                        failFast = new NotImplementedException($"Have to implement configuration format converters ('{configVersion}' -> '{applicationVersion}').");
                    }
                }
                else
                    IsBrandNewConfiguration = true;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"An error occured when trying to load configuration file '{filename ?? "(null)"}'.");
            }

            if (failFast != null)
                throw failFast;

            IConfiguration defaultInstance = new T
            {
                Version = applicationVersion
            };

            return (T)defaultInstance;
        }

        private static int ReadFileVersion(string content)
        {
            var root = JsonConvert.DeserializeObject(content) as JObject;
            if (root == null)
                return -1;

            if (root.TryGetValue("version", out JToken versionToken) == false)
                return -1;

            if (versionToken is JValue versionValue && versionToken.Type == JTokenType.Integer)
                return versionToken.Value<int>();

            return -1;
        }
    }
}
