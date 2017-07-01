using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataMover.Parsers;

namespace DataMover
{
    public class Record : IRecord
    {
        private readonly Dictionary<string, IParser> _record = new Dictionary<string, IParser>();
        IEnumerator<KeyValuePair<string, IParser>> IEnumerable<KeyValuePair<string, IParser>>.GetEnumerator()
        {
            return _record.GetEnumerator();
        }

        IEnumerator<IParser> IEnumerable<IParser>.GetEnumerator()
        {
            return _record.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _record.Values.GetEnumerator();
        }

        public void Add(KeyValuePair<string, IParser> item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, IParser> item)
        {
            return _record.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, IParser>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, IParser> item)
        {
            throw new System.NotImplementedException();
        }

        public int Count => _record.Count;
        public bool IsReadOnly => true;

        public void Add(string key, IParser value)
        {
            throw new System.NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return _record.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetValue(string key, out IParser value)
        {
            return _record.TryGetValue(key, out value);
        }

        public IParser this[string key]
        {
            get => _record[key];
            set => throw new System.NotImplementedException();
        }

        public ICollection<string> Keys => _record.Keys;
        public ICollection<IParser> Values => _record.Values;
        public void Parse(IEnumerable<string> source)
        {
            throw new System.NotImplementedException();
        }
    }
}