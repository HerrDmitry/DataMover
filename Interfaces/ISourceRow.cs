using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public interface ISourceRow
    {
        IEnumerable<StringBuilder>[] Columns { get; }
        StringBuilder Source { get; }
        bool HasError { get; }
        string Error { get; }
        long RowNumber { get; }
        long ParsedLineNumber { get; }
        long RawLineNumber { get; }
    }
}
