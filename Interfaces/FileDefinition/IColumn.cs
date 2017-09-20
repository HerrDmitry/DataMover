namespace Interfaces.FileDefinition
{
    public interface IColumn
    {
        string Name { get; }
        string Format { get; }
        ColumnType Type { get; }
    }
}