using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MHArmory.DataSource.DataStructures
{
    public class SkillPrimitive
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("abilityIds")]
        public IList<int> AbilityIds { get; set; }
    }
}
