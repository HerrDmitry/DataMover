namespace Interfaces
{
    public interface ICsvFormatConfiguration:IFileFormatConfiguration
    {
        char[] ColumnDelimiter { get; }
        char[] StringQualifier { get; }
        char[] LineDelimiter { get; }
    }
}