using System.Collections.Generic;

namespace Interfaces.Configuration
{
    public interface IFile:ICsvFile,IFileMedia
    {
        string Name { get; }
        string Format { get; }
        IList<IRow> Rows { get; }
    }

    public interface ICsvFile:IHasNullableColumns
    {
        string Delimiter { get; }
        string Qualifier { get; }
    }

    public interface IFileMedia
    {
        MediaType MediaType { get; }
        string Login { get; }
        string Password { get; }
        string Path { get; }
        bool IncludeSubfolders { get; }
    }

    public interface IHasNullableColumns
    {
        string NullValue { get; }
    }
}