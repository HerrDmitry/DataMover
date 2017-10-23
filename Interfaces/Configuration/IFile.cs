using System.Collections.Generic;

namespace Interfaces.Configuration
{
    public interface IFile:IFileConfiguration
    {
        IFileMedia Media { get; }
        IList<IFileMedia> MultipleMedia { get; }
    }

    public interface IFileConfiguration:ICsvFile,IFixedWidthFile
    {
        FileFormat Format { get; }
        bool TrimStrings { get; }
        bool Disabled { get; }
        IList<IRow> Rows { get; }
    }

    public interface ICsvFile:IHasNullableColumns,IFileName
    {
        bool HasHeaders { get; }
        string Delimiter { get; }
        string Qualifier { get; }
        bool ForceQualifier { get; }
        SurroundedQualifierType SurroundedQualifier { get; }
        bool FixForExcel { get; }
    }
    
    public interface IFixedWidthFile:IFileName
    {
        bool HasLineDelimiters { get; }
    }

    public interface IFileName
    {
        string Name { get; }
    }

    public interface IFileMedia
    {
        MediaType MediaType { get; }
        string Path { get; }
        bool IncludeSubfolders { get; }
        DataOperation Operation { get; }
        string Credentials { get; }
        bool Disabled { get; }
        ICredentials ConnectionCredentials { get; }
        FileEncoding Encoding { get; }
    }

    public interface ICredentials
    {
        string Name { get; }
        string Login { get; }
        string Password { get; }
        string Token { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string EntryPoint { get; }
    }

    public interface IHasNullableColumns
    {
        string NullValue { get; }
    }
}