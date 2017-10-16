using System.Collections.Generic;

namespace Interfaces.Configuration
{
    public interface IColumn
    {
        string Name { get; }
        string Format { get; }
        ColumnType Type { get; }
        int Width { get; }
        CalendarType? CalendarType { get; }
    }
}