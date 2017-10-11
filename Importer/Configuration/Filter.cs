using Interfaces.Configuration;
using Newtonsoft.Json;

namespace Importer.Configuration
{
    public class Filter : IFilter
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}