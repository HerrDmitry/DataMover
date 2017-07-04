using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataMover.Configuration
{
    public class Row
    {
        [JsonProperty("columns")]
        public List<Column> Columns { get; set; }
    }
}