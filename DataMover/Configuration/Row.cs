using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataMover.Configuration
{
    public class Row : IEnumerable<Column>
    {
        [JsonProperty("columns")]
        public List<Column> Columns { get; set; }

        public Row(IEnumerable<Column> columns)
        {
            Columns = new List<Column>(columns);
        }

        public IEnumerator<Column> GetEnumerator()
        {
            return Columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Columns.GetEnumerator();
        }
    }
}