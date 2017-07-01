using System.Collections;
using System.Collections.Generic;

namespace DataMover.Configuration
{
    public class Record : IEnumerable<Row>
    {
        private readonly List<Row> _rows;

        public Record(IEnumerable<Row> rows)
        {
            _rows = new List<Row>(rows);
        }

        public IEnumerator<Row> GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rows.GetEnumerator();
        }
    }
}