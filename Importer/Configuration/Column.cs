using System.Collections.Generic;
using Interfaces;
using Interfaces.Configuration;
using Newtonsoft.Json;

namespace Importer.Configuration
{
    public class Column:IColumn
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("format")]
        public string Format { get; set; }
        [JsonProperty("type")]
        public ColumnType Type { get; set; }
    }
}