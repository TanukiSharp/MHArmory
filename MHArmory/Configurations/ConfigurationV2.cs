using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MHArmory.Configurations
{
    public class WindowConfiguration
    {
        [JsonProperty("isMaximized")]
        public bool IsMaximized { get; set; }
        [JsonProperty("left")]
        public int? Left { get; set; }
        [JsonProperty("top")]
        public int? Top { get; set; }
        [JsonProperty("width")]
        public int? Width { get; set; }
        [JsonProperty("height")]
        public int? Height { get; set; }
    }

    public class DecorationOverrideConfigurationV2
    {
        [JsonProperty("useOverride")]
        public bool UseOverride { get; set; }
        [JsonProperty("decorationOverrides")]
        public Dictionary<string, DecorationOverrideConfigurationItem> Items { get; } = new Dictionary<string, DecorationOverrideConfigurationItem>();
    }

    public class EquipmentOverrideConfigurationV2
    {
        [JsonProperty("useOverride")]
        public bool UseOverride { get; set; }
        [JsonProperty("isStoringPossessed")]
        public bool IsStoringPossessed { get; set; }
        [JsonProperty("equipmentOverrides")]
        public IList<string> Items { get; } = new List<string>();
    }

    public class InParametersConfigurationV2
    {
        [JsonProperty("weaponSlots")]
        public int[] WeaponSlots { get; set; }
        [JsonProperty("decorationOverride")]
        public DecorationOverrideConfigurationV2 DecorationOverride { get; } = new DecorationOverrideConfigurationV2();
        [JsonProperty("equipmentOverride")]
        public EquipmentOverrideConfigurationV2 EquipmentOverride { get; } = new EquipmentOverrideConfigurationV2();
        [JsonProperty("rarity")]
        public int Rarity { get; set; }
        [JsonProperty("gender")]
        public int Gender { get; set; }
    }

    public class SkillLoadoutItemConfigurationV2
    {
        [JsonProperty("skill")]
        public string SkillName { get; set; }
        [JsonProperty("level")]
        public int Level { get; set; }
    }

    public class ConfigurationV2 : IConfiguration
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
        public Dictionary<string, SkillLoadoutItemConfigurationV2[]> SkillLoadouts { get; } = new Dictionary<string, SkillLoadoutItemConfigurationV2[]>();

        [JsonProperty("searchResultProcessing")]
        public SearchResultProcessingConfiguration SearchResultProcessing { get; } = new SearchResultProcessingConfiguration();

        [JsonProperty("inParameters")]
        public InParametersConfigurationV2 InParameters { get; } = new InParametersConfigurationV2();

        [JsonProperty("windows")]
        public Dictionary<string, WindowConfiguration> Windows { get; } = new Dictionary<string, WindowConfiguration>();
    }
}
