using DataMover.Configuration.JsonConverters;
using Newtonsoft.Json;

namespace DataMover.Configuration
{
    public class Column
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("format")]
        public string Format { get; set; }
        [JsonProperty("type")]
        [JsonConverter(typeof(ColumnTypeConverter))]
        public ColumnType Type { get; set; }
    }
}