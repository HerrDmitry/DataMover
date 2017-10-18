using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Interfaces
{
    public interface IRawLine
    {
        IEnumerable<StringBuilder> Columns { get; }
        StringBuilder Source { get; }
        long RowNumber { get; }
        string SourcePath { get; }

    }
}