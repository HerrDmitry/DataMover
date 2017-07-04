using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataMover.Configuration
{
    public class Source
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        public int ReaderLines { get; set; }
        public int FooterLines { get; set;  }
        [JsonProperty("records")]
        public List<Record> Records { get; set; }
    }
}