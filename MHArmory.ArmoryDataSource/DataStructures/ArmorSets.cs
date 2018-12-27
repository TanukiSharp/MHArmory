using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MHArmory.ArmoryDataSource.DataStructures
{
    public class FullArmorSetPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("armorPieceIds")]
        public IList<int> ArmorPieceIds { get; set; }
    }

    public class ArmorSetSkillPartPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("requiredArmorPieceCount")]
        public int RequiredArmorPieceCount { get; set; }
        [JsonProperty("abilityIds")]
        public IList<int> GrantedSkills { get; set; }
    }

    public class ArmorSetSkillPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public Dictionary<string, string> Name { get; set; }
        [JsonProperty("parts")]
        public IList<ArmorSetSkillPartPrimitive> Parts { get; set; }
    }
}
