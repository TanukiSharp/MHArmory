using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MHArmory.Search.Testing
{
    public class SampleModel
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("num1")]
        public int Numeric { get; set; }
        [JsonProperty("num2")]
        public int AnotherNumeric { get; set; }
    }
}
