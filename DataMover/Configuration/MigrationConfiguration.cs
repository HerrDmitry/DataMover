using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataMover.Configuration
{
    public class MigrationConfiguration
    {
        [JsonProperty("sources")]
        public List<Source> Sources { get; set; }

        [JsonProperty("files")]
        public List<File> Files { get; set; }
    }
    
}