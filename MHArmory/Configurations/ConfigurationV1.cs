using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MHArmory.Configurations
{
    public class DecorationOverrideConfigurationItem
    {
        [JsonProperty("isOverriding")]
        public bool IsOverriding { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class DecorationOverrideConfiguration
    {
        [JsonProperty("useOverride")]
        public bool UseOverride { get; set; }
        [JsonProperty("decorationOverrides")]
        public Dictionary<int, DecorationOverrideConfigurationItem> Items { get; } = new Dictionary<int, DecorationOverrideConfigurationItem>();
    }

    public class InParametersConfiguration
    {
        [JsonProperty("weaponSlots")]
        public int[] WeaponSlots { get; set; }

        [JsonProperty("decorationOverride")]
        public DecorationOverrideConfiguration DecorationOverride { get; } = new DecorationOverrideConfiguration();

        [JsonProperty("rarity")]
        public int Rarity { get; set; }
    }

    public class SearchResultSortItemConfiguration
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("criterias")]
        public SearchResultSortCriteria[] Criterias { get; set; }
    }

    public class SearchResultProcessingConfiguration
    {
        [JsonProperty("sortingActiveIndex")]
        public int ActiveSortingIndex { get; set; } = -1;
        [JsonProperty("sorting")]
        public SearchResultSortItemConfiguration[] Sorting { get; set; }
    }

    public class ConfigurationV1 : IConfiguration
    {
        [JsonProperty("version")]
        public uint Version { get; set; }

        [JsonProperty("backupLocations")]
        public string[] BackupLocations { get; set; }

        [JsonProperty("lastOpenedLoadout")]
        public string LastOpenedLoadout { get; set; }

        [JsonProperty("skillLoadouts")]
        public Dictionary<string, int[]> SkillLoadouts { get; } = new Dictionary<string, int[]>();

        [JsonProperty("searchResultProcessing")]
        public SearchResultProcessingConfiguration SearchResultProcessing { get; } = new SearchResultProcessingConfiguration();

        [JsonProperty("inParameters")]
        public InParametersConfiguration InParameters { get; } = new InParametersConfiguration();
    }
}
