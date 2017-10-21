using System.Collections.Generic;

namespace Interfaces.Configuration
{
    public interface IColumn
    {
        string Name { get; }
        string Alias { get; }
        string Format { get; }
        ColumnType Type { get; }
        int Width { get; }
        CalendarType? CalendarType { get; }
        string Description { get; }
        bool GroupKey { get; }
        AggregateMethod AggregateMethod { get; }
    }
}