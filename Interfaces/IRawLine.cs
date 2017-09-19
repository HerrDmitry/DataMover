using System.Collections.Generic;
using System.ComponentModel;

namespace Interfaces
{
    public interface IRawLine
    {
        IEnumerable<string> Columns { get; }
        string Source { get; }
        long RowNumber { get; }
    }
}