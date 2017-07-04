using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataMover.Configuration
{
    public class Record
    {
        [JsonProperty("rows")]
        public List<Row> Rows { get; set; }
    }
}