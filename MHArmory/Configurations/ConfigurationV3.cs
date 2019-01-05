using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MHArmory.Configurations
{
    public class SkillLoadoutItemConfigurationV3
    {
        [JsonProperty("weaponSlots")]
        public int[] WeaponSlots { get; set; }
        [JsonProperty("skills")]
        public SkillLoadoutItemConfigurationV2[] Skills { get; set; }
    }

    public class ConfigurationV3 : IConfiguration
    {
        [JsonProperty("version")]
        public uint Version { get; set; }

        [JsonProperty("acknowledgedVersion")]
        public string AcknowledgedVersion { get; set; }

        [JsonProperty("backupLocations")]
        public string[] BackupLocations { get; set; }

        [JsonProperty("lastOpenedLoadout")]
        public string LastOpenedLoadout { get; set; }

        [JsonProperty("skillLoadouts")]
        public Dictionary<string, SkillLoadoutItemConfigurationV3> SkillLoadouts { get; } = new Dictionary<string, SkillLoadoutItemConfigurationV3>();

        [JsonProperty("searchResultProcessing")]
        public SearchResultProcessingConfiguration SearchResultProcessing { get; } = new SearchResultProcessingConfiguration();

        [JsonProperty("inParameters")]
        public InParametersConfigurationV2 InParameters { get; } = new InParametersConfigurationV2();

        [JsonProperty("windows")]
        public Dictionary<string, WindowConfiguration> Windows { get; } = new Dictionary<string, WindowConfiguration>();
    }
}
