namespace Interfaces
{
    public interface IColumn
    {
        string Name { get; }
        string Format { get; }
        ColumnType Type { get; }
    }
}