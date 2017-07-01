using System.Collections;
using System.Collections.Generic;

namespace DataMover.Configuration
{
    public class Row : IEnumerable<Column>
    {
        private readonly List<Column> _columns;

        public Row(IEnumerable<Column> columns)
        {
            _columns = new List<Column>(columns);
        }

        public IEnumerator<Column> GetEnumerator()
        {
            return _columns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _columns.GetEnumerator();
        }
    }
}