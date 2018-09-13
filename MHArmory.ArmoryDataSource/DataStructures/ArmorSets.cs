using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MHArmory.DataSource.DataStructures
{
    public class ArmorSetSkillPrimitive
    {
        [JsonProperty("requiredArmorPieceCount")]
        public int RequiredArmorPieceCount { get; set; }
        [JsonProperty("abilityIds")]
        public IList<int> GrantedSkills { get; set; }
    }

    public class ArmorSetPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("full")]
        public bool IsFull { get; set; }
        [JsonProperty("armorPieceIds")]
        public IList<int> ArmorPieceIds { get; set; }
        [JsonProperty("setSkills")]
        public IList<ArmorSetSkillPrimitive> Skills { get; set; }
    }
}
