using System.Collections.Generic;

namespace DataMover.Configuration
{
    public class Source
    {
        public string Name { get; }
        public string Path { get; }
        public int ReaderLines { get; }
        public int FooterLines { get; }
        public IEnumerable<Record> Records { get; }
    }
}