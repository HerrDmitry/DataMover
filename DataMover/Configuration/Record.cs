using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataMover.Configuration
{
    public class Record : IEnumerable<Row>
    {
        [JsonProperty("rows")]
        public List<Row> Rows { get; set; }

        public Record(IEnumerable<Row> rows)
        {
            Rows = new List<Row>(rows);
        }

        public IEnumerator<Row> GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Rows.GetEnumerator();
        }
    }
}