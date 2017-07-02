using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataMover.Configuration
{
    public class Source
    {
        public string Name { get; }
        public string Path { get; }
        public int ReaderLines { get; }
        public int FooterLines { get; }
        [JsonProperty("records")]
        public IEnumerable<Record> Records { get; }
    }
}