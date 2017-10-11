namespace Interfaces.Configuration
{
    public interface IColumn
    {
        string Name { get; }
        string Format { get; }
        ColumnType Type { get; }
    }
}