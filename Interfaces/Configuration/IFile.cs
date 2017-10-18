using System.Collections.Generic;

namespace Interfaces.Configuration
{
    public interface IFile:ICsvFile,IFileMedia,IFixedWidthFile
    {
        string Name { get; }
        FileFormat Format { get; }
        IList<IRow> Rows { get; }
        bool TrimStrings { get; }
        bool Disabled { get; }
    }

    public interface ICsvFile:IHasNullableColumns
    {
        string Delimiter { get; }
        string Qualifier { get; }
        bool ForceQualifier { get; }
        SurroundedQualifierType SurroundedQualifier { get; }
    }
    
    public interface IFixedWidthFile{
        bool HasLineDelimiters { get; }
    }

    public interface IFileMedia
    {
        MediaType MediaType { get; }
        string Login { get; }
        string Password { get; }
        string Token { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string Path { get; }
        bool IncludeSubfolders { get; }
        DataOperation Operation { get; }
    }

    public interface IHasNullableColumns
    {
        string NullValue { get; }
    }
}