using DataMover.Configuration.JsonConverters;
using Newtonsoft.Json;

namespace DataMover.Configuration
{
    public class File
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(FileTypeConverter))]
        public FileType Type { get; set; }

        [JsonProperty("textQualifier")]
        public string TextQualifier { get; set; }

        [JsonProperty("delimiter")]
        public string Delimiter { get; set; }
    }
}