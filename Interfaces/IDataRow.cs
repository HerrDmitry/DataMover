using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public interface IDataRow
    {
        IDictionary<string, IValue> Columns { get; }
        string Error { get; }
        long RowNumber { get; }
        long RawLineNumber { get; }
        IValue this[string key] { get; }
        string SourcePath { get; }
    }
}
