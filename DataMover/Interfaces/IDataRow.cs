using System;
using System.Collections.Generic;
using System.Text;

namespace DataMover.Interfaces
{
    public interface ISourceRow
    {
        string[] Columns { get; }
        string Source { get; }
        bool HasError { get; }
        string Error { get; }
    }
}
