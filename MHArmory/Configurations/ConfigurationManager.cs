using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MHArmory.Configurations
{
    public static class ConfigurationManager
    {
        private static ILogger logger;

        public static void SetLogger(ILogger logger)
        {
            ConfigurationManager.logger = logger;
        }

        public static void Save(IConfiguration configurationObject)
        {
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
                            Directory.CreateDirectory(path);

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

            if (uint.TryParse(versionStr, out uint version) == false)
                throw new InvalidOperationException($"Invalid configuration type provided '{typeof(T).FullName}', invalid version number '{versionStr}'.");

            string filename = null;
            Exception failFast = null;

            try
            {
                filename = Path.Combine(AppContext.BaseDirectory, "config.json");
                if (File.Exists(filename))
                {
                    string content = File.ReadAllText(filename);

                    IConfiguration config = JsonConvert.DeserializeObject<T>(content);

                    if (config.Version == 0)
                        config.Version = version;

                    if (config.Version == version)
                        return (T)config;

                    if (config.Version > version)
                        failFast = new InvalidDataException($"Invalid configuration file version '{config.Version}', latest supported version is '{version}'.");
                    else
                    {
                        // TODO Here version is between 1 and 'version - 1', implement auto conversion of configuration formats
                        failFast = new NotImplementedException($"Have to implement configuration format converters ('{config.Version}' -> '{version}').");
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"An error occured when trying to load configuration file '{filename ?? "(null)"}'.");
            }

            if (failFast != null)
                throw failFast;

            IConfiguration defaultInstance = new T
            {
                Version = version
            };

            return (T)defaultInstance;
        }
    }
}
