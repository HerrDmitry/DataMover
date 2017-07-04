using System.Collections.Generic;
using System.IO;

namespace DataMover.Loaders
{
    public interface ILoader
    {
        IEnumerable<DataRow> ReadLines(Stream source);
    }
}
