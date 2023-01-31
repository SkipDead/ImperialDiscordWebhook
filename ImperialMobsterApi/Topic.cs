using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImperialWebHook
{
    public class Topic
    {
        [JsonProperty("topics")]
        public List<string> topics { get; set; }
    }
}
