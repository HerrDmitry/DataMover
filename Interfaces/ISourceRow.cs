using System;
using System.Collections.Generic;

namespace Interfaces
{
    public interface ISourceRow
    {
        IEnumerable<string>[] Columns { get; }
        string Source { get; }
        bool HasError { get; }
        string Error { get; }
        long RowNumber { get; }
        long ParsedLineNumber { get; }
        long RawLineNumber { get; }
    }
}
