using System.Collections.Generic;

namespace Interfaces.FileDefinition
{
    public interface IFile
    {
        string Name { get; }
        string NullValue { get; }
        IList<IRecord> Records { get; }
    }

    public interface ICsvFile : IFile
    {
        string Delimiter { get; }
        string Qualifier { get; }
    }
}